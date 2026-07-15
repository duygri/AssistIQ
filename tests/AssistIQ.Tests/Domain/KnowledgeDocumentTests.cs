using AssistIQ.Domain.Knowledge;
using FluentAssertions;

namespace AssistIQ.Tests.Domain;

public sealed class KnowledgeDocumentTests
{
    [Fact]
    public void CreateIndexing_ShouldCaptureUploadedDocumentMetadata()
    {
        var uploadedByUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var document = KnowledgeDocument.CreateIndexing(
            fileName: "billing-faq.pdf",
            contentType: "application/pdf",
            sizeBytes: 120_000,
            textContent: "Billing renews monthly.",
            uploadedByUserId,
            now);

        document.FileName.Should().Be("billing-faq.pdf");
        document.ContentType.Should().Be("application/pdf");
        document.SizeBytes.Should().Be(120_000);
        document.TextContent.Should().Be("Billing renews monthly.");
        document.UploadedByUserId.Should().Be(uploadedByUserId);
        document.Status.Should().Be(KnowledgeDocumentStatus.Indexing);
        document.UploadedAt.Should().Be(now);
        document.ProviderFileId.Should().BeNull();
        document.ProviderVectorStoreId.Should().BeNull();
    }

    [Fact]
    public void MarkReady_ShouldMoveIndexingDocumentToReady()
    {
        var document = KnowledgeDocument.CreateIndexing(
            "refund-policy.pdf",
            "application/pdf",
            98_000,
            "Refunds are reviewed within two business days.",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-2));
        var indexedAt = DateTimeOffset.UtcNow;

        document.MarkReady("vs_123", "file_456", indexedAt);

        document.Status.Should().Be(KnowledgeDocumentStatus.Ready);
        document.ProviderVectorStoreId.Should().Be("vs_123");
        document.ProviderFileId.Should().Be("file_456");
        document.IndexedAt.Should().Be(indexedAt);
        document.ErrorSummary.Should().BeNull();
    }

    [Fact]
    public void MarkReady_ShouldRejectDisabledDocument()
    {
        var document = KnowledgeDocument.CreateIndexing(
            "old-guide.pdf",
            "application/pdf",
            42_000,
            "Legacy support guide.",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        document.MarkReady("vs_old", "file_old", DateTimeOffset.UtcNow);
        document.Disable(DateTimeOffset.UtcNow);

        var act = () => document.MarkReady("vs_123", "file_456", DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Disabled*");
    }

    [Fact]
    public void MarkFailed_ShouldCaptureFailureSummary()
    {
        var document = KnowledgeDocument.CreateIndexing(
            "broken.pdf",
            "application/pdf",
            1_024,
            "Broken provider content.",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        var failedAt = DateTimeOffset.UtcNow.AddSeconds(5);

        document.MarkFailed("Provider timeout", failedAt);

        document.Status.Should().Be(KnowledgeDocumentStatus.Failed);
        document.ErrorSummary.Should().Be("Provider timeout");
        document.IndexedAt.Should().Be(failedAt);
    }
}
