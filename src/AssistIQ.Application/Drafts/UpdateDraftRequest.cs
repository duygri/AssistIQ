using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Drafts;

public sealed record UpdateDraftRequest([Required, StringLength(8_000)] string EditedAnswer);
