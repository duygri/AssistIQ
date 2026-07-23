using System.Text;
using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Knowledge;

namespace AssistIQ.Application.Knowledge;

public sealed class KnowledgeDocumentService(
    IKnowledgeDocumentRepository repository,
    IKnowledgeIndexer indexer,
    IAuditService auditService,
    ICurrentUser currentUser,
    ISystemClock clock)
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxTextContentCharacters = 20_000;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md",
        ".txt",
        ".pdf"
    };

    public async Task<KnowledgeDocumentDto> RegisterAsync(
        RegisterKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRegistration(request);

        var document = KnowledgeDocument.CreateIndexing(
            request.FileName,
            request.ContentType,
            request.SizeBytes,
            request.TextContent,
            currentUser.UserId,
            clock.UtcNow);

        await repository.AddAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await indexer.IndexAsync(
                request.FileName,
                request.ContentType,
                request.SizeBytes,
                request.TextContent,
                cancellationToken);

            document.MarkReady(result.ProviderVectorStoreId, result.ProviderFileId, clock.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);

            await auditService.RecordAsync(
                currentUser.UserId,
                AuditAction.KnowledgeDocumentUploaded,
                nameof(KnowledgeDocument),
                document.Id,
                before: null,
                after: ToDto(document),
                cancellationToken);

            return ToDto(document);
        }
        catch (Exception)
        {
            document.MarkFailed(ErrorCodes.IndexingFailed, clock.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);

            throw new AppException(502, ErrorCodes.IndexingFailed, "Knowledge document indexing failed.");
        }
    }

    public async Task<IReadOnlyList<KnowledgeDocumentDto>> ListAsync(CancellationToken cancellationToken)
    {
        var documents = await repository.ListAsync(cancellationToken);
        return documents.Select(ToDto).ToArray();
    }

    public async Task<PagedResult<KnowledgeDocumentDto>> ListPagedAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListPagedAsync(pagination.Skip, pagination.PageSize, cancellationToken);
        return new PagedResult<KnowledgeDocumentDto>(items.Select(ToDto).ToArray(), total, pagination.Page, pagination.PageSize);
    }

    public async Task<KnowledgeDocumentDto> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await repository.FindByIdAsync(id, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, "Knowledge document was not found.");

        if (document.ProviderVectorStoreId is null || document.ProviderFileId is null)
        {
            throw new AppException(409, ErrorCodes.Conflict, "Knowledge document does not have provider ids.");
        }

        var before = ToDto(document);

        try
        {
            await indexer.DisableAsync(document.ProviderVectorStoreId, document.ProviderFileId, cancellationToken);
        }
        catch (Exception)
        {
            throw new AppException(502, ErrorCodes.IndexingFailed, "Knowledge document provider disable failed.");
        }

        try
        {
            document.Disable(clock.UtcNow);
        }
        catch (InvalidOperationException)
        {
            throw new AppException(409, ErrorCodes.Conflict, "Knowledge document cannot be disabled in its current state.");
        }

        await repository.SaveChangesAsync(cancellationToken);

        await auditService.RecordAsync(
            currentUser.UserId,
            AuditAction.KnowledgeDocumentDisabled,
            nameof(KnowledgeDocument),
            document.Id,
            before,
            ToDto(document),
            cancellationToken);

        return ToDto(document);
    }

    private static void ValidateRegistration(RegisterKnowledgeDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new AppException(400, ErrorCodes.ValidationFailed, "File name is required.");
        }

        if (!SupportedExtensions.Contains(Path.GetExtension(request.FileName)))
        {
            throw new AppException(400, ErrorCodes.UnsupportedDocumentFormat, "Only .md, .txt, and .pdf are supported.");
        }

        if (request.SizeBytes > MaxFileSizeBytes ||
            request.TextContent.Length > MaxTextContentCharacters ||
            Encoding.UTF8.GetByteCount(request.TextContent) > MaxFileSizeBytes)
        {
            throw new AppException(400, ErrorCodes.DocumentTooLarge, "Knowledge document must be 5 MB or smaller.");
        }

        if (request.SizeBytes <= 0 || string.IsNullOrWhiteSpace(request.TextContent))
        {
            throw new AppException(400, ErrorCodes.ValidationFailed, "Document content is required.");
        }
    }

    private static KnowledgeDocumentDto ToDto(KnowledgeDocument document)
    {
        return new KnowledgeDocumentDto(
            document.Id,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.Status,
            document.ProviderVectorStoreId,
            document.ProviderFileId,
            document.ErrorSummary,
            document.UploadedAt,
            document.IndexedAt,
            document.DisabledAt);
    }
}
