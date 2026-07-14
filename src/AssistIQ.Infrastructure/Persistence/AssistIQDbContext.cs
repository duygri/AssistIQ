using AssistIQ.Domain.Audit;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Knowledge;
using AssistIQ.Domain.Tickets;
using AssistIQ.Domain.Usage;
using AssistIQ.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AssistIQ.Infrastructure.Persistence;

public sealed class AssistIQDbContext(DbContextOptions<AssistIQDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<Draft> Drafts => Set<Draft>();

    public DbSet<DraftCitation> DraftCitations => Set<DraftCitation>();

    public DbSet<UsageLog> UsageLogs => Set<UsageLog>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssistIQDbContext).Assembly);
    }
}
