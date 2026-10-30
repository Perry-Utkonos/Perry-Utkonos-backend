namespace Chat.Application;

public sealed class ChatException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
