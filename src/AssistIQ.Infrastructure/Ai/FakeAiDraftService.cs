using AssistIQ.Application.Abstractions;

namespace AssistIQ.Infrastructure.Ai;

public sealed class FakeAiDraftService : IAiDraftService
{
    public Task<AiDraftResult> GenerateAsync(
        TicketDraftInput input,
        CancellationToken cancellationToken)
    {
        var citations = input.Sources
            .Select(source => new AiCitationResult(
                source.KnowledgeDocumentId,
                source.FileName,
                source.ProviderFileId,
                source.QuoteOrExcerpt,
                source.ProviderResultId,
                source.Confidence))
            .ToArray();

        var answer = citations.Length == 0
            ? $"I need a reviewed knowledge source before sending a final answer for: {input.CustomerQuestion}"
            : $"Thanks for reaching out. Based on our support knowledge, {input.CustomerQuestion.Trim()} can be handled using the cited policy.";

        return Task.FromResult(new AiDraftResult(
            answer,
            citations,
            "fake-ai",
            "fake-support-copilot-v1",
            $"resp_{input.TicketId:N}",
            InputTokens: 320,
            OutputTokens: 180));
    }
}
