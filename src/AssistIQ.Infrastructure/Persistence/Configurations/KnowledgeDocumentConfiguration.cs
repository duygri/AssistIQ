using AssistIQ.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("knowledge_documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.Id).HasColumnName("id");
        builder.Property(document => document.FileName).HasColumnName("file_name").HasMaxLength(260).IsRequired();
        builder.Property(document => document.ContentType).HasColumnName("content_type").HasMaxLength(120).IsRequired();
        builder.Property(document => document.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(document => document.UploadedByUserId).HasColumnName("uploaded_by_user_id").IsRequired();
        builder.Property(document => document.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(document => document.ProviderVectorStoreId).HasColumnName("provider_vector_store_id").HasMaxLength(160);
        builder.Property(document => document.ProviderFileId).HasColumnName("provider_file_id").HasMaxLength(160);
        builder.Property(document => document.ErrorSummary).HasColumnName("error_summary").HasMaxLength(1_000);
        builder.Property(document => document.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Property(document => document.IndexedAt).HasColumnName("indexed_at");
        builder.Property(document => document.DisabledAt).HasColumnName("disabled_at");

        builder.HasIndex(document => document.Status);
        builder.HasIndex(document => document.ProviderFileId);
    }
}
