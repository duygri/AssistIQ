using AssistIQ.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssistIQ.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Id).HasColumnName("id");
        builder.Property(ticket => ticket.CustomerQuestion).HasColumnName("customer_question").HasMaxLength(4_000).IsRequired();
        builder.Property(ticket => ticket.CustomerName).HasColumnName("customer_name").HasMaxLength(160);
        builder.Property(ticket => ticket.CustomerEmail).HasColumnName("customer_email").HasMaxLength(320);
        builder.Property(ticket => ticket.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(ticket => ticket.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(ticket => ticket.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ticket => ticket.DraftedAt).HasColumnName("drafted_at");
        builder.Property(ticket => ticket.SentAt).HasColumnName("sent_at");

        builder.HasIndex(ticket => ticket.Status);
        builder.HasIndex(ticket => ticket.CreatedAt);
    }
}
