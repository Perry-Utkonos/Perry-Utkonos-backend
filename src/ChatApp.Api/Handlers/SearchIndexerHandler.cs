namespace ChatApp.Api.Handlers;

using ChatApp.Application.Events;

public class SearchIndexerHandler : IEventHandler<MessageCreatedEvent>
{
    private readonly ILogger<SearchIndexerHandler> _logger;
    public SearchIndexerHandler(ILogger<SearchIndexerHandler> logger) => _logger = logger;

    public Task HandleAsync(MessageCreatedEvent evt, CancellationToken ct = default)
    {
        // TODO: отправить в Elasticsearch/Meilisearch
        _logger.LogInformation("[SEARCH] index message {MsgId} seq={Seq}", evt.MessageId, evt.Seq);
        return Task.CompletedTask;
    }
}
