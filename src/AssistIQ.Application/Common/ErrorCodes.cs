namespace AssistIQ.Application.Common;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Unauthorized = "unauthorized";
    public const string UnsupportedDocumentFormat = "unsupported_document_format";
    public const string DocumentTooLarge = "document_too_large";
    public const string IndexingFailed = "indexing_failed";
    public const string NoReadyKnowledgeDocument = "no_ready_knowledge_document";
    public const string DraftGenerationFailed = "draft_generation_failed";
    public const string DraftNeedsCitationReview = "draft_needs_citation_review";
}
