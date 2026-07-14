using AssistIQ.Application.Abstractions;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Ai;

public sealed class FakeRetrievalService(AssistIQDbContext dbContext) : IRetrievalService
{
    public async Task<IReadOnlyList<RetrievedSource>> RetrieveAsync(
        TicketRetrievalInput input,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.KnowledgeDocuments
            .AsNoTracking()
            .Where(document => document.Status == KnowledgeDocumentStatus.Ready && document.ProviderFileId != null)
            .OrderBy(document => document.FileName)
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            return [];
        }

        return
        [
            new RetrievedSource(
                document.Id,
                document.FileName,
                document.ProviderFileId!,
                $"Relevant support policy excerpt from {document.FileName}.",
                "fake_result_1",
                0.91m)
        ];
    }
}
