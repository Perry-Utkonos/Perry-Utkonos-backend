using ZodiacThreadsManager.Core.Models;

namespace ZodiacThreadsManager.Core.Services;

public interface IThreadService
{
    string Name { get; }
    Task ProcessAsync(ThreadMessage message, CancellationToken ct = default);
}
