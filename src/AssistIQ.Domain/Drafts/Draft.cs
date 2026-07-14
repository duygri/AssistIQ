namespace AssistIQ.Domain.Drafts;

public sealed class Draft
{
    private readonly List<DraftCitation> _citations = [];

    private Draft()
    {
        GeneratedAnswer = string.Empty;
    }

    private Draft(
        Guid id,
        Guid ticketId,
        int versionNumber,
        string generatedAnswer,
        DraftSource source,
        DraftStatus status,
        IEnumerable<DraftCitation> citations,
        DateTimeOffset createdAt)
    {
        Id = id;
        TicketId = ticketId;
        VersionNumber = versionNumber;
        GeneratedAnswer = generatedAnswer;
        Source = source;
        Status = status;
        CreatedAt = createdAt;
        _citations.AddRange(citations);
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    public int VersionNumber { get; private set; }

    public DraftSource Source { get; private set; }

    public DraftStatus Status { get; private set; }

    public string GeneratedAnswer { get; private set; }

    public string? EditedAnswer { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? EditedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public IReadOnlyCollection<DraftCitation> Citations => _citations.AsReadOnly();

    public static Draft CreateAiGenerated(
        Guid ticketId,
        int versionNumber,
        string generatedAnswer,
        IReadOnlyCollection<DraftCitation> citations)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Draft version number must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(generatedAnswer))
        {
            throw new InvalidOperationException("Generated answer is required.");
        }

        var status = citations.Count == 0
            ? DraftStatus.NeedsCitationReview
            : DraftStatus.Generated;

        return new Draft(
            Guid.NewGuid(),
            ticketId,
            versionNumber,
            generatedAnswer.Trim(),
            DraftSource.AiGenerated,
            status,
            citations,
            DateTimeOffset.UtcNow);
    }

    public void Edit(string editedAnswer)
    {
        if (Status == DraftStatus.Sent)
        {
            throw new InvalidOperationException("Sent drafts cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(editedAnswer))
        {
            throw new InvalidOperationException("Edited answer is required.");
        }

        EditedAnswer = editedAnswer.Trim();
        Status = DraftStatus.Edited;
        EditedAt = DateTimeOffset.UtcNow;
    }

    public void Send()
    {
        if (Status == DraftStatus.NeedsCitationReview || _citations.Count == 0)
        {
            throw new InvalidOperationException("Draft cannot be sent without at least one citation.");
        }

        if (Status == DraftStatus.Sent)
        {
            throw new InvalidOperationException("Draft is already Sent.");
        }

        Status = DraftStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }
}
