namespace AssistIQ.Application.Tickets;

public sealed record CreateTicketRequest(
    string CustomerQuestion,
    string? CustomerName,
    string? CustomerEmail);
