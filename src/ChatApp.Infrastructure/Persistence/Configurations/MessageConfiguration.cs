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

        // КРИТИЧНО: (conversation_id, seq) должен быть уникален
        b.HasIndex(x => new { x.ConversationId, x.Seq }).IsUnique();

        b.HasOne(x => x.Conversation)
         .WithMany(c => c.Messages)
         .HasForeignKey(x => x.ConversationId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Sender)
         .WithMany(u => u.Messages)
         .HasForeignKey(x => x.SenderId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.ReplyTo)
         .WithMany(m => m.Replies)
         .HasForeignKey(x => x.ReplyToId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
