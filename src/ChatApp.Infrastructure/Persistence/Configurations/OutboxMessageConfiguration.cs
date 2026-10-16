namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Type).HasColumnName("type").IsRequired().HasMaxLength(64);
        b.Property(x => x.Payload).HasColumnName("payload").IsRequired().HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.PublishedAt).HasColumnName("published_at");
        b.Property(x => x.Error).HasColumnName("error").HasMaxLength(2000);
        b.Property(x => x.Attempts).HasColumnName("attempts").HasDefaultValue(0);
    }
}
