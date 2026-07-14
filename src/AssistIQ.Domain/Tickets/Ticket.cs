namespace AssistIQ.Domain.Tickets;

public sealed class Ticket
{
    private Ticket()
    {
        CustomerQuestion = string.Empty;
        CustomerEmail = string.Empty;
    }

    private Ticket(
        Guid id,
        string customerQuestion,
        string? customerName,
        string? customerEmail,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        CustomerQuestion = customerQuestion;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = TicketStatus.Open;
    }

    public Guid Id { get; private set; }

    public string CustomerQuestion { get; private set; }

    public string? CustomerName { get; private set; }

    public string? CustomerEmail { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DraftedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public static Ticket Create(
        string customerQuestion,
        string? customerName,
        string? customerEmail,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerQuestion))
        {
            throw new InvalidOperationException("Ticket question is required.");
        }

        return new Ticket(
            Guid.NewGuid(),
            customerQuestion.Trim(),
            string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim(),
            string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim(),
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
