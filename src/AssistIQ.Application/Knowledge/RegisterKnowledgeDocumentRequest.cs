using System.ComponentModel.DataAnnotations;

namespace AssistIQ.Application.Knowledge;

public sealed record RegisterKnowledgeDocumentRequest(
    [Required, StringLength(260)] string FileName,
    [Required, StringLength(120)] string ContentType,
    [Range(1, 5 * 1024 * 1024)] long SizeBytes,
    [Required, StringLength(20_000)] string TextContent);
