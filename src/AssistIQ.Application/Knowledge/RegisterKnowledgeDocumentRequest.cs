namespace AssistIQ.Application.Knowledge;

public sealed record RegisterKnowledgeDocumentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    string TextContent);
