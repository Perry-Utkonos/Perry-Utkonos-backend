namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("attachments");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.MessageId).HasColumnName("message_id");
        b.Property(x => x.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
        b.Property(x => x.MimeType).HasColumnName("mime_type").IsRequired().HasMaxLength(128);
        b.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        b.Property(x => x.StorageKey).HasColumnName("storage_key").IsRequired().HasMaxLength(500);
        b.Property(x => x.Width).HasColumnName("width");
        b.Property(x => x.Height).HasColumnName("height");
        b.Property(x => x.Checksum).HasColumnName("checksum").HasMaxLength(64);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");

        b.HasIndex(x => x.MessageId);

        b.HasOne(x => x.Message)
         .WithMany(m => m.Attachments)
         .HasForeignKey(x => x.MessageId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
