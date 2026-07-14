using AssistIQ.Domain.Usage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class UsageLogConfiguration : IEntityTypeConfiguration<UsageLog>
{
    public void Configure(EntityTypeBuilder<UsageLog> builder)
    {
        builder.ToTable("usage_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id).HasColumnName("id");
        builder.Property(log => log.ActorUserId).HasColumnName("actor_user_id").IsRequired();
        builder.Property(log => log.TicketId).HasColumnName("ticket_id");
        builder.Property(log => log.DraftId).HasColumnName("draft_id");
        builder.Property(log => log.Provider).HasColumnName("provider").HasMaxLength(80).IsRequired();
        builder.Property(log => log.Model).HasColumnName("model").HasMaxLength(120).IsRequired();
        builder.Property(log => log.ResponseId).HasColumnName("response_id").HasMaxLength(160);
        builder.Property(log => log.PromptTokens).HasColumnName("input_tokens").IsRequired();
        builder.Property(log => log.CompletionTokens).HasColumnName("output_tokens").IsRequired();
        builder.Property(log => log.TotalTokens).HasColumnName("total_tokens").IsRequired();
        builder.Property(log => log.EstimatedCost).HasColumnName("estimated_cost").HasPrecision(18, 8).IsRequired();
        builder.Property(log => log.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(log => log.ErrorSummary).HasColumnName("error_summary").HasMaxLength(1_000);
        builder.Property(log => log.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => log.Status);
        builder.HasIndex(log => log.TicketId);
    }
}
