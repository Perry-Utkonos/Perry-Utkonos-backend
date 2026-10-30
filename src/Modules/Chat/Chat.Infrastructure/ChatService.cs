using System.Text.Json;
using Chat.Application;
using Chat.Contracts;
using Chat.Domain;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure;

internal sealed class ChatService(ChatDbContext db, IChatPolicy policy) : IChatService
{
    public async Task<Conversation> RequireMember(Guid id, Guid user, CancellationToken ct)
    {
        return await db.Conversations.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == id && (x.UserA == user || x.UserB == user), ct)
            ?? throw new ChatException(404, "Диалог не найден");
    }
    public async Task<Conversation> Create(Guid actor, Guid peer, CancellationToken ct)
    {
        if (peer == Guid.Empty || actor == peer) throw new ChatException(400, "Некорректный участник");
        if (!await policy.CanCreate(actor, peer, ct)) throw new ChatException(403, "Создание диалога запрещено");
        var a = actor.CompareTo(peer) < 0 ? actor : peer;
        var z = a == actor ? peer : actor;
        var id = Guid.NewGuid(); var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO chat."Conversations" ("Id", "UserA", "UserB", "Sequence", "CreatedAt")
            VALUES ({id}, {a}, {z}, 0, {now}) ON CONFLICT ("UserA", "UserB") DO NOTHING
            """, ct);
        return await db.Conversations.AsNoTracking().SingleAsync(x => x.UserA == a && x.UserB == z, ct);
    }
    public async Task<Message> Send(Guid id, Guid actor, SendRequest request, CancellationToken ct)
    {
        if (request is null || request.ClientMessageId == Guid.Empty || string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 4000)
            throw new ChatException(400, "Нужен идентификатор и текст от 1 до 4000 символов");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        // Serialize writes to a conversation, including retries with identical client IDs.
        var rows = await db.Conversations.FromSqlInterpolated($"SELECT * FROM chat.\"Conversations\" WHERE \"Id\" = {id} FOR UPDATE").ToListAsync(ct);
        var c = rows.SingleOrDefault();
        if (c is null || (c.UserA != actor && c.UserB != actor)) throw new ChatException(404, "Диалог не найден");
        var previous = await db.Messages.SingleOrDefaultAsync(x => x.ConversationId == id && x.SenderId == actor && x.ClientMessageId == request.ClientMessageId, ct);
        if (previous is not null)
        {
            if (previous.Body != request.Body) throw new ChatException(409, "Идентификатор уже использован с другим текстом");
            await tx.CommitAsync(ct); return previous;
        }
        if (!await policy.CanSend(actor, c.UserA == actor ? c.UserB : c.UserA, ct)) throw new ChatException(403, "Отправка запрещена");
        var message = new Message { Id = Guid.NewGuid(), ConversationId = id, SenderId = actor,
            ClientMessageId = request.ClientMessageId, Sequence = ++c.Sequence, Body = request.Body, CreatedAt = DateTime.UtcNow };
        db.Messages.Add(message);
        db.Outbox.Add(new OutboxEvent { Id = Guid.NewGuid(), ConversationId = id, CreatedAt = DateTime.UtcNow,
            Payload = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web)) });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return message;
    }
    public async Task MarkRead(Guid id, Guid actor, long sequence, CancellationToken ct)
    {
        var c = await RequireMember(id, actor, ct);
        if (sequence < 0 || sequence > c.Sequence) throw new ChatException(400, "Некорректная позиция прочтения");
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO chat."Reads" ("ConversationId", "UserId", "Sequence") VALUES ({id}, {actor}, {sequence})
            ON CONFLICT ("ConversationId", "UserId") DO UPDATE
            SET "Sequence" = GREATEST(chat."Reads"."Sequence", EXCLUDED."Sequence")
            """, ct);
    }

    public async Task<List<Conversation>> List(Guid actor, CancellationToken ct) =>
        await db.Conversations.AsNoTracking().Where(x => x.UserA == actor || x.UserB == actor)
            .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Take(100).ToListAsync(ct);

    public async Task<List<Message>> History(Guid id, Guid actor, long? before, long? after, int limit, CancellationToken ct)
    {
        await RequireMember(id, actor, ct);
        if (before.HasValue && after.HasValue) throw new ChatException(400, "Укажите before или after");
        if (before < 0 || after < 0 || limit < 1 || limit > 100) throw new ChatException(400, "Некорректные параметры страницы");
        var query = db.Messages.AsNoTracking().Where(x => x.ConversationId == id);
        if (before.HasValue) query = query.Where(x => x.Sequence < before.Value);
        if (after.HasValue) query = query.Where(x => x.Sequence > after.Value);
        var ordered = after.HasValue ? query.OrderBy(x => x.Sequence) : query.OrderByDescending(x => x.Sequence);
        return await ordered.Take(limit).ToListAsync(ct);
    }

    public async Task<List<ReadPosition>> GetReads(Guid id, Guid actor, CancellationToken ct)
    {
        await RequireMember(id, actor, ct);
        return await db.Reads.AsNoTracking().Where(x => x.ConversationId == id).ToListAsync(ct);
    }
}
