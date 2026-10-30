namespace Chat.Contracts;

public sealed record SendRequest(Guid ClientMessageId, string Body);
public sealed record CreateRequest(Guid PeerId);
public sealed record ReadRequest(long Sequence);
