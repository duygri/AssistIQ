namespace AssistIQ.Domain.Tickets;

public sealed class Ticket
{
    private Ticket()
    {
        CustomerEmail = string.Empty;
        Subject = string.Empty;
        Question = string.Empty;
    }

    private Ticket(
        Guid id,
        string customerEmail,
        string subject,
        string question,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        CustomerEmail = customerEmail;
        Subject = subject;
        Question = question;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = TicketStatus.Open;
    }

    public Guid Id { get; private set; }

    public string CustomerEmail { get; private set; }

    public string Subject { get; private set; }

    public string Question { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DraftedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public static Ticket Create(
        string customerEmail,
        string subject,
        string question,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new InvalidOperationException("Customer email is required.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Ticket subject is required.");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new InvalidOperationException("Ticket question is required.");
        }

        return new Ticket(
            Guid.NewGuid(),
            customerEmail.Trim(),
            subject.Trim(),
            question.Trim(),
            createdByUserId,
            createdAt);
    }

    public void MarkDrafted(DateTimeOffset draftedAt)
    {
        if (Status == TicketStatus.Sent)
        {
            throw new InvalidOperationException("Sent tickets cannot be drafted again.");
        }

        Status = TicketStatus.Drafted;
        DraftedAt = draftedAt;
    }

    public void MarkSent(DateTimeOffset sentAt)
    {
        if (Status != TicketStatus.Drafted)
        {
            throw new InvalidOperationException("Only Drafted tickets can be Sent.");
        }

        Status = TicketStatus.Sent;
        SentAt = sentAt;
    }
}
