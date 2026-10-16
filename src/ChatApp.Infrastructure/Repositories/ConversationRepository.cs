namespace ChatApp.Infrastructure.Repositories;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ConversationRepository : IConversationRepository
{
    private readonly ChatDbContext _db;

    public ConversationRepository(ChatDbContext db) => _db = db;

    public async Task<IReadOnlyList<ConversationDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var convs = await _db.Conversations
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Include(c => c.UserA).ThenInclude(u => u.Profile)
            .Include(c => c.UserB).ThenInclude(u => u.Profile)
            .OrderByDescending(c => c.LastMessageAt)
            .Take(100)
            .ToListAsync(ct);

        var convIds = convs.Select(c => c.Id).ToList();

        var reads = await _db.MessageReads
            .Where(r => r.UserId == userId && convIds.Contains(r.ConversationId))
            .ToDictionaryAsync(r => r.ConversationId, r => r.LastReadSeq, ct);

        var unreadExact = new Dictionary<Guid, int>();
        foreach (var c in convs)
        {
            var lastRead = reads.GetValueOrDefault(c.Id, 0);
            var count = await _db.Messages.CountAsync(m =>
                m.ConversationId == c.Id
                && m.Seq > lastRead
                && m.SenderId != userId
                && m.DeletedAt == null, ct);
            unreadExact[c.Id] = count;
        }

        return convs.Select(c => new ConversationDto(
            c.Id,
            ToUserDto(c.UserAId == userId ? c.UserB : c.UserA),
            c.LastMessageBody,
            c.LastMessageAt,
            c.LastMessageId,
            c.LastSeq,
            unreadExact.GetValueOrDefault(c.Id, 0)
        )).ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetConversationIdsAsync(Guid userId, CancellationToken ct = default)
        => await _db.Conversations
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Select(c => c.Id)
            .ToListAsync(ct);

    public async Task<ConversationDto> CreateOrGetAsync(Guid currentUserId, Guid userA, Guid userB, CancellationToken ct = default)
    {
        var existing = await _db.Conversations
            .Include(c => c.UserA).ThenInclude(u => u.Profile)
            .Include(c => c.UserB).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(c => c.UserAId == userA && c.UserBId == userB, ct);

        if (existing is not null)
            return ToDto(existing, currentUserId);

        var conv = new Conversation
        {
            UserAId = userA,
            UserBId = userB
        };
        _db.Conversations.Add(conv);

        _db.MessageReads.Add(new MessageRead { ConversationId = conv.Id, UserId = userA, LastReadSeq = 0 });
        _db.MessageReads.Add(new MessageRead { ConversationId = conv.Id, UserId = userB, LastReadSeq = 0 });

        await _db.SaveChangesAsync(ct);

        var peer = await _db.Users.Include(u => u.Profile)
            .FirstAsync(u => u.Id == (currentUserId == userA ? userB : userA), ct);

        return new ConversationDto(conv.Id, ToUserDto(peer), null, null, null, 0, 0);
    }

    public async Task<MessagePageDto> GetMessagesAsync(Guid conversationId, long? beforeSeq, int limit, CancellationToken ct = default)
    {
        var query = _db.Messages
            .Where(m => m.ConversationId == conversationId && m.DeletedAt == null);

        if (beforeSeq.HasValue)
            query = query.Where(m => m.Seq < beforeSeq.Value);

        var items = await query
            .OrderByDescending(m => m.Seq)
            .Take(limit + 1)
            .Select(m => new MessageDto(
                m.Id, m.ConversationId, m.SenderId, m.Seq,
                m.Body, m.Type, m.ReplyToId,
                m.CreatedAt, m.EditedAt, m.DeletedAt))
            .ToListAsync(ct);

        var hasMore = items.Count > limit;
        if (hasMore) items = items.Take(limit).ToList();

        items.Reverse();

        return new MessagePageDto(
            items,
            hasMore ? items.First().Seq : null,
            hasMore);
    }

    public async Task EnsureParticipantAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var isParticipant = await _db.Conversations.AnyAsync(c =>
            c.Id == conversationId && (c.UserAId == userId || c.UserBId == userId), ct);

        if (!isParticipant)
            throw new ForbiddenException("You are not a participant of this conversation.");
    }

    private static ConversationDto ToDto(Conversation c, Guid currentUserId)
    {
        var peer = c.UserAId == currentUserId ? c.UserB : c.UserA;
        return new ConversationDto(
            c.Id, ToUserDto(peer),
            c.LastMessageBody, c.LastMessageAt, c.LastMessageId,
            c.LastSeq, 0);
    }

    private static UserDto ToUserDto(User u) => new(
        u.Id, u.Username, u.Email,
        u.Profile?.Bio, u.Profile?.AvatarUrl, u.LastSeenAt);
}
