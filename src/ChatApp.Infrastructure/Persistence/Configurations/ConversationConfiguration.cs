namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("conversations", t =>
        {
            // CHECK: user_a < user_b — гарантия уникальности пары
            t.HasCheckConstraint("chk_conv_order", "user_a_id < user_b_id");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.UserAId).HasColumnName("user_a_id");
        b.Property(x => x.UserBId).HasColumnName("user_b_id");
        b.Property(x => x.LastSeq).HasColumnName("last_seq").HasDefaultValue(0L);
        b.Property(x => x.LastMessageId).HasColumnName("last_message_id");
        b.Property(x => x.LastMessageAt).HasColumnName("last_message_at");
        b.Property(x => x.LastMessageBody).HasColumnName("last_message_body").HasMaxLength(500);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        // UNIQUE(user_a_id, user_b_id) — один диалог на пару
        b.HasIndex(x => new { x.UserAId, x.UserBId }).IsUnique();
        b.HasIndex(x => new { x.UserAId, x.LastMessageAt });
        b.HasIndex(x => new { x.UserBId, x.LastMessageAt });

        b.HasOne(x => x.UserA)
         .WithMany(u => u.ConversationsAsUserA)
         .HasForeignKey(x => x.UserAId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.UserB)
         .WithMany(u => u.ConversationsAsUserB)
         .HasForeignKey(x => x.UserBId)
         .OnDelete(DeleteBehavior.Cascade);

        // Денормализованный указатель на последнее сообщение
        b.HasOne(x => x.LastMessage)
         .WithMany()
         .HasForeignKey(x => x.LastMessageId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
