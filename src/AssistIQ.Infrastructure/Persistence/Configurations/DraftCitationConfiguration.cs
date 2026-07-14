using AssistIQ.Domain.Drafts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class DraftCitationConfiguration : IEntityTypeConfiguration<DraftCitation>
{
    public void Configure(EntityTypeBuilder<DraftCitation> builder)
    {
        builder.ToTable("draft_citations");

        builder.HasKey(citation => citation.Id);

        builder.Property(citation => citation.Id).HasColumnName("id");
        builder.Property(citation => citation.DraftId).HasColumnName("draft_id").IsRequired();
        builder.Property(citation => citation.KnowledgeDocumentId).HasColumnName("knowledge_document_id").IsRequired();
        builder.Property(citation => citation.FileName).HasColumnName("file_name").HasMaxLength(260).IsRequired();
        builder.Property(citation => citation.ProviderFileId).HasColumnName("provider_file_id").HasMaxLength(160).IsRequired();
        builder.Property(citation => citation.Quote).HasColumnName("quote").HasMaxLength(2_000).IsRequired();
        builder.Property(citation => citation.ProviderResultId).HasColumnName("provider_result_id").HasMaxLength(160);
        builder.Property(citation => citation.Confidence).HasColumnName("confidence").HasPrecision(5, 4);

        builder.HasIndex(citation => citation.DraftId);
        builder.HasIndex(citation => citation.KnowledgeDocumentId);
    }
}
