using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Drafts;

public sealed record GenerateDraftRequest([StringLength(1_000)] string? Instructions);
