namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("messages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ConversationId).HasColumnName("conversation_id");
        b.Property(x => x.SenderId).HasColumnName("sender_id");
        b.Property(x => x.Seq).HasColumnName("seq");
        b.Property(x => x.Body).HasColumnName("body").HasMaxLength(8000);
        b.Property(x => x.Type).HasColumnName("message_type").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.ReplyToId).HasColumnName("reply_to_id");
        b.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.EditedAt).HasColumnName("edited_at");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        b.HasIndex(x => new { x.ConversationId, x.Seq }).IsUnique();

        b.HasOne(x => x.Conversation).WithMany(c => c.Messages)
            .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sender).WithMany(u => u.Messages)
            .HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ReplyTo).WithMany(m => m.Replies)
            .HasForeignKey(x => x.ReplyToId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class MessageReadConfiguration : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(EntityTypeBuilder<MessageRead> b)
    {
        b.ToTable("message_reads");
        b.HasKey(x => new { x.ConversationId, x.UserId });
        b.Property(x => x.ConversationId).HasColumnName("conversation_id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.LastReadSeq).HasColumnName("last_read_seq").HasDefaultValue(0L);
        b.Property(x => x.LastReadAt).HasColumnName("last_read_at");

        b.HasOne(x => x.Conversation).WithMany(c => c.Reads)
            .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany(u => u.MessageReads)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

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
        b.HasOne(x => x.Message).WithMany(m => m.Attachments)
            .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
    }
}
