namespace ChatApp.Infrastructure.Repositories;

using System.Data;
using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Application.Events;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _db;
    private readonly IEventBus _bus;

    public MessageRepository(ChatDbContext db, IEventBus bus)
    {
        _db = db;
        _bus = bus;
    }

    public async Task<MessageDto> SendAsync(
        Guid conversationId, Guid senderId, Guid clientMessageId, string body,
        CancellationToken ct = default)
    {
        var existing = await _db.Messages.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == clientMessageId, ct);
        if (existing is not null) return ToDto(existing);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var seq = await IncrementSeqAsync(conversationId, senderId, ct);
        if (seq is null)
            throw new ForbiddenException("Not a participant of this conversation.");

        var now = DateTime.UtcNow;
        var message = new Message
        {
            Id = clientMessageId,
            ConversationId = conversationId,
            SenderId = senderId,
            Seq = seq.Value,
            Body = body,
            Type = Domain.Enums.MessageType.Text,
            CreatedAt = now
        };
        _db.Messages.Add(message);

        var conv = await _db.Conversations.FirstAsync(c => c.Id == conversationId, ct);
        conv.LastMessageId = message.Id;
        conv.LastMessageAt = now;
        conv.LastMessageBody = body.Length > 500 ? body[..500] : body;
        conv.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await _bus.PublishAsync(new MessageCreatedEvent(
            message.Id, message.ConversationId, message.SenderId,
            message.Seq, message.Body ?? string.Empty), ct);

        return ToDto(message);
    }

    private async Task<long?> IncrementSeqAsync(Guid conversationId, Guid senderId, CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        var openedHere = conn.State != ConnectionState.Open;
        if (openedHere) await conn.OpenAsync(ct);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = _db.Database.CurrentTransaction!.GetDbTransaction();
            cmd.CommandText = @"
                UPDATE conversations
                   SET last_seq = last_seq + 1
                 WHERE id = @conv AND (user_a_id = @user OR user_b_id = @user)
             RETURNING last_seq";

            var pConv = cmd.CreateParameter(); pConv.ParameterName = "@conv"; pConv.Value = conversationId;
            cmd.Parameters.Add(pConv);
            var pUser = cmd.CreateParameter(); pUser.ParameterName = "@user"; pUser.Value = senderId;
            cmd.Parameters.Add(pUser);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result is null or DBNull ? null : Convert.ToInt64(result);
        }
        finally
        {
            if (openedHere) await conn.CloseAsync();
        }
    }

    public async Task MarkReadAsync(Guid conversationId, Guid userId, long lastReadSeq, CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO message_reads (conversation_id, user_id, last_read_seq, last_read_at)
            VALUES ({0}, {1}, {2}, now())
            ON CONFLICT (conversation_id, user_id)
            DO UPDATE SET
                last_read_seq = GREATEST(message_reads.last_read_seq, EXCLUDED.last_read_seq),
                last_read_at  = now()",
            new object[] { conversationId, userId, lastReadSeq }, ct);
    }

    public async Task EnsureParticipantAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var ok = await _db.Conversations.AnyAsync(c =>
            c.Id == conversationId && (c.UserAId == userId || c.UserBId == userId), ct);
        if (!ok) throw new ForbiddenException("Not a participant.");
    }

    private static MessageDto ToDto(Message m) => new(
        m.Id, m.ConversationId, m.SenderId, m.Seq,
        m.Body, m.Type, m.ReplyToId,
        m.CreatedAt, m.EditedAt, m.DeletedAt);
}
