using AssistIQ.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id).HasColumnName("id");
        builder.Property(log => log.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(log => log.Action).HasColumnName("action").HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(log => log.EntityName).HasColumnName("entity_name").HasMaxLength(120).IsRequired();
        builder.Property(log => log.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(log => log.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(log => log.BeforeJson).HasColumnName("before_json").HasColumnType("jsonb");
        builder.Property(log => log.AfterJson).HasColumnName("after_json").HasColumnType("jsonb");
        builder.Property(log => log.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");

        builder.HasIndex(log => log.OccurredAt);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => log.Action);
    }
}
