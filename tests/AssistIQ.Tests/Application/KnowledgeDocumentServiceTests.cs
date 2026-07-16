using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Common;
using AssistIQ.Application.Knowledge;
using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Domain.Users;
using AssistIQ.Infrastructure.Ai;
using AssistIQ.Infrastructure.Persistence;
using AssistIQ.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Tests.Application;

public sealed class KnowledgeDocumentServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithUnsupportedExtension_ShouldReturnValidationError()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();

        var act = () => scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "guide.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            1_000,
            "content"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.UnsupportedDocumentFormat);
    }

    [Fact]
    public async Task RegisterAsync_WithFileAboveFiveMb_ShouldReturnValidationError()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();

        var act = () => scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "large.pdf",
            "application/pdf",
            5 * 1024 * 1024 + 1,
            "content"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.DocumentTooLarge);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyContent_ShouldReturnValidationError()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();

        var act = () => scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "empty.md",
            "text/markdown",
            256,
            " "),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task RegisterAsync_WithOversizedTextContent_ShouldReturnValidationError()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();

        var act = () => scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "large.md",
            "text/markdown",
            1,
            new string('x', 20_001)),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.DocumentTooLarge);
    }

    [Fact]
    public async Task RegisterAsync_WithValidDocument_ShouldCreateReadyDocumentAndAuditLog()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();

        var dto = await scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "billing-faq.md",
            "text/markdown",
            512,
            "Billing renews monthly."),
            CancellationToken.None);

        dto.Status.Should().Be(KnowledgeDocumentStatus.Ready);
        dto.ProviderVectorStoreId.Should().Be("vs_demo");
        dto.ProviderFileId.Should().StartWith("file_");

        var document = await scope.DbContext.KnowledgeDocuments.SingleAsync();
        document.Status.Should().Be(KnowledgeDocumentStatus.Ready);

        var retrieval = new FakeRetrievalService(scope.DbContext);
        var sources = await retrieval.RetrieveAsync(
            new TicketRetrievalInput(Guid.NewGuid(), "When does billing renew?"),
            CancellationToken.None);
        sources.Should().ContainSingle();
        sources[0].QuoteOrExcerpt.Should().Be("Billing renews monthly.");

        var audit = await scope.DbContext.AuditLogs.SingleAsync();
        audit.Action.Should().Be(AuditAction.KnowledgeDocumentUploaded);
    }

    [Fact]
    public async Task RetrieveAsync_WithMultipleDocuments_ShouldSelectRelevantSource()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();
        await scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "billing.md",
            "text/markdown",
            128,
            "Billing renews monthly."),
            CancellationToken.None);
        await scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "password.md",
            "text/markdown",
            128,
            "Passwords can be reset from account settings."),
            CancellationToken.None);
        var retrieval = new FakeRetrievalService(scope.DbContext);

        var sources = await retrieval.RetrieveAsync(
            new TicketRetrievalInput(Guid.NewGuid(), "How can I reset my password?"),
            CancellationToken.None);

        sources.Should().ContainSingle();
        sources[0].FileName.Should().Be("password.md");
    }

    [Fact]
    public async Task RetrieveAsync_WithoutRelevantDocument_ShouldReturnNoSources()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();
        await scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "billing.md",
            "text/markdown",
            128,
            "Billing renews monthly."),
            CancellationToken.None);
        var retrieval = new FakeRetrievalService(scope.DbContext);

        var sources = await retrieval.RetrieveAsync(
            new TicketRetrievalInput(Guid.NewGuid(), "Where can I change my profile photo?"),
            CancellationToken.None);

        sources.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WhenIndexerFails_ShouldMarkDocumentFailed()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();
        scope.Indexer.ShouldFailIndexing = true;

        var act = () => scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "broken.txt",
            "text/plain",
            128,
            "content"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.IndexingFailed);

        var document = await scope.DbContext.KnowledgeDocuments.SingleAsync();
        document.Status.Should().Be(KnowledgeDocumentStatus.Failed);
        document.ErrorSummary.Should().Be(ErrorCodes.IndexingFailed);
        document.ErrorSummary.Should().NotContain("Fake indexer");
    }

    [Fact]
    public async Task DisableAsync_WhenProviderFails_ShouldLeaveDocumentReady()
    {
        await using var scope = await KnowledgeDocumentTestScope.CreateAsync();
        var dto = await scope.Service.RegisterAsync(new RegisterKnowledgeDocumentRequest(
            "policy.pdf",
            "application/pdf",
            256,
            "Support policy."),
            CancellationToken.None);
        scope.Indexer.ShouldFailDisable = true;

        var act = () => scope.Service.DisableAsync(dto.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(exception => exception.ErrorCode == ErrorCodes.IndexingFailed);

        var document = await scope.DbContext.KnowledgeDocuments.SingleAsync();
        document.Status.Should().Be(KnowledgeDocumentStatus.Ready);
    }

    private sealed class KnowledgeDocumentTestScope : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private KnowledgeDocumentTestScope(
            SqliteConnection connection,
            AssistIQDbContext dbContext,
            FakeKnowledgeIndexer indexer,
            KnowledgeDocumentService service)
        {
            _connection = connection;
            DbContext = dbContext;
            Indexer = indexer;
            Service = service;
        }

        public AssistIQDbContext DbContext { get; }

        public FakeKnowledgeIndexer Indexer { get; }

        public KnowledgeDocumentService Service { get; }

        public static async Task<KnowledgeDocumentTestScope> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AssistIQDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AssistIQDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var indexer = new FakeKnowledgeIndexer();
            var clock = new FixedClock(new DateTimeOffset(2026, 7, 14, 9, 0, 0, TimeSpan.Zero));
            var currentUser = new FixedCurrentUser(Guid.NewGuid(), "admin@assistiq.local", UserRole.Admin);
            var auditService = new AuditService(dbContext, clock);
            var repository = new KnowledgeDocumentRepository(dbContext);
            var service = new KnowledgeDocumentService(repository, indexer, auditService, currentUser, clock);

            return new KnowledgeDocumentTestScope(connection, dbContext, indexer, service);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : ISystemClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class FixedCurrentUser(Guid userId, string email, UserRole role) : ICurrentUser
    {
        public Guid UserId { get; } = userId;

        public string Email { get; } = email;

        public UserRole Role { get; } = role;
    }
}
