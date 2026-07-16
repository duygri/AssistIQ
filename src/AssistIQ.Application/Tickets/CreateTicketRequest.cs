using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Tickets;

public sealed record CreateTicketRequest(
    [Required, StringLength(4_000)] string CustomerQuestion,
    [StringLength(160)] string? CustomerName,
    [EmailAddress, StringLength(320)] string? CustomerEmail);
