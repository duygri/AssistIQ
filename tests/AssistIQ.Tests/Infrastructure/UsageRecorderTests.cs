using AssistIQ.Application.Abstractions;
using AssistIQ.Infrastructure.Ai;
using AssistIQ.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AssistIQ.Tests.Infrastructure;

public sealed class UsageRecorderTests
{
    [Fact]
    public async Task RecordSucceededAsync_ForGitHubModelsFreeProvider_ShouldStoreZeroCost()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var dbOptions = new DbContextOptionsBuilder<AssistIQDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new AssistIQDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();
        var recorder = new UsageRecorder(
            dbContext,
            new FixedClock(),
            Options.Create(new UsageCostOptions()));

        var log = await recorder.RecordSucceededAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "github-models",
            "openai/gpt-4.1",
            "chatcmpl-demo",
            1_000,
            500,
            CancellationToken.None);

        log.EstimatedCost.Should().Be(0m);
    }

    private sealed class FixedClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 16, 0, 0, 0, TimeSpan.Zero);
    }
}
