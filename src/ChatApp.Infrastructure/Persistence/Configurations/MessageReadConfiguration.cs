namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MessageReadConfiguration : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(EntityTypeBuilder<MessageRead> b)
    {
        b.ToTable("message_reads");

        // Составной PK: одна строка на пару (conversation, user)
        b.HasKey(x => new { x.ConversationId, x.UserId });

        b.Property(x => x.ConversationId).HasColumnName("conversation_id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.LastReadSeq).HasColumnName("last_read_seq").HasDefaultValue(0L);
        b.Property(x => x.LastReadAt).HasColumnName("last_read_at");

        b.HasOne(x => x.Conversation)
         .WithMany(c => c.Reads)
         .HasForeignKey(x => x.ConversationId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User)
         .WithMany(u => u.MessageReads)
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
