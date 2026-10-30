using Chat.Contracts;
using Chat.Domain;

namespace Chat.Application;

public interface IChatService
{
    Task<Conversation> RequireMember(Guid id, Guid user, CancellationToken ct);
    Task<Conversation> Create(Guid actor, Guid peer, CancellationToken ct);
    Task<Message> Send(Guid id, Guid actor, SendRequest request, CancellationToken ct);
    Task MarkRead(Guid id, Guid actor, long sequence, CancellationToken ct);
    Task<List<Conversation>> List(Guid actor, CancellationToken ct);
    Task<List<Message>> History(Guid id, Guid actor, long? before, long? after, int limit, CancellationToken ct);
    Task<List<ReadPosition>> GetReads(Guid id, Guid actor, CancellationToken ct);
}
