using Chat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ReadPosition> Reads => Set<ReadPosition>();
    public DbSet<OutboxEvent> Outbox => Set<OutboxEvent>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("chat");
        b.Entity<Conversation>().ToTable("Conversations");
        b.Entity<Message>().ToTable("Messages");
        b.Entity<ReadPosition>().ToTable("Reads");
        b.Entity<OutboxEvent>().ToTable("Outbox");
        b.Entity<Conversation>().HasIndex(x => new { x.UserA, x.UserB }).IsUnique();
        b.Entity<Conversation>().HasIndex(x => x.UserB);
        b.Entity<Message>().HasIndex(x => new { x.ConversationId, x.Sequence }).IsUnique();
        b.Entity<Message>().HasIndex(x => new { x.ConversationId, x.SenderId, x.ClientMessageId }).IsUnique();
        b.Entity<Message>().Property(x => x.Body).HasMaxLength(4000);
        b.Entity<Message>().HasOne<Conversation>().WithMany().HasForeignKey(x => x.ConversationId);
        b.Entity<ReadPosition>().HasKey(x => new { x.ConversationId, x.UserId });
        b.Entity<ReadPosition>().HasOne<Conversation>().WithMany().HasForeignKey(x => x.ConversationId);
        b.Entity<OutboxEvent>().HasIndex(x => x.CreatedAt).HasFilter("\"ProcessedAt\" IS NULL");
    }
}
