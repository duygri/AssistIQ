using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Ai;

public sealed class FakeRetrievalService(AssistIQDbContext dbContext) : IRetrievalService
{
    private const int MaxExcerptCharacters = 4_000;

    public async Task<IReadOnlyList<RetrievedSource>> RetrieveAsync(
        TicketRetrievalInput input,
        CancellationToken cancellationToken)
    {
        var documents = await dbContext.KnowledgeDocuments
            .AsNoTracking()
            .Where(document => document.Status == KnowledgeDocumentStatus.Ready &&
                document.ProviderFileId != null &&
                document.TextContent != string.Empty)
            .ToListAsync(cancellationToken);

        if (documents.Count == 0)
        {
            return [];
        }

        var terms = input.CustomerQuestion
            .Split([' ', '\t', '\r', '\n', '.', ',', '?', '!', ':', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 4)
            .Select(term => term.ToLowerInvariant())
            .Distinct()
            .ToArray();
        var match = documents
            .Select(candidate => new
            {
                Document = candidate,
                Score = Score(candidate.FileName, candidate.TextContent, terms)
            })
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Document.FileName)
            .First();
        if (match.Score == 0)
        {
            return [];
        }

        var document = match.Document;
        var excerptLength = Math.Min(document.TextContent.Length, MaxExcerptCharacters);

        return
        [
            new RetrievedSource(
                document.Id,
                document.FileName,
                document.ProviderFileId!,
                document.TextContent[..excerptLength],
                "fake_result_1",
                0.91m)
        ];
    }

    private static int Score(string fileName, string textContent, IReadOnlyList<string> terms)
    {
        var searchable = $"{fileName}\n{textContent}".ToLowerInvariant();
        return terms.Count(term => searchable.Contains(term, StringComparison.Ordinal));
    }
}
