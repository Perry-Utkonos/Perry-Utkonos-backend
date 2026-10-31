namespace Chat.Domain;

public sealed class ReadPosition
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public long Sequence { get; set; }
}
