using AssistIQ.Domain.Drafts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder)
    {
        builder.ToTable("drafts");

        builder.HasKey(draft => draft.Id);

        builder.Property(draft => draft.Id).HasColumnName("id");
        builder.Property(draft => draft.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(draft => draft.VersionNumber).HasColumnName("version_number").IsRequired();
        builder.Property(draft => draft.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(draft => draft.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(draft => draft.GeneratedAnswer).HasColumnName("generated_answer").HasMaxLength(8_000).IsRequired();
        builder.Property(draft => draft.EditedAnswer).HasColumnName("edited_answer").HasMaxLength(8_000);
        builder.Property(draft => draft.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(draft => draft.EditedAt).HasColumnName("edited_at");
        builder.Property(draft => draft.SentAt).HasColumnName("sent_at");

        builder.HasIndex(draft => new { draft.TicketId, draft.VersionNumber }).IsUnique();
        builder.HasIndex(draft => draft.Status);

        builder.HasMany(draft => draft.Citations)
            .WithOne()
            .HasForeignKey(citation => citation.DraftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(draft => draft.Citations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
