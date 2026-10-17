using System.Threading.Channels;

public class TaskQueue
{
    // Абсолютный потолок — жёсткая защита от OOM
    private const int MaxCapacity = 20_000;
    // Минимальная «мягкая» ёмкость
    private const int MinCapacity = 100;
    // Начальная «мягкая» ёмкость
    private const int InitialCapacity = 500;

    private readonly Channel<string> _queue;
    private readonly object _capacityLock = new();
    private int _capacity = InitialCapacity;

    public TaskQueue()
    {
        // Bounded + FullMode.Wait = естественный backpressure.
        // Когда очередь заполнится до MaxCapacity, WriteAsync начнёт
        // ждать освобождения, а не жрать память.
        _queue = Channel.CreateBounded<string>(new BoundedChannelOptions(MaxCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    /// <summary>Текущий адаптивный мягкий лимит.</summary>
    public int Capacity
    {
        get { lock (_capacityLock) return _capacity; }
    }

    /// <summary>Сколько сейчас в очереди.</summary>
    public int Count => _queue.Reader.Count;

    /// <summary>Мягкий лимит достигнут — продюсеру стоит притормозить.</summary>
    public bool IsOverloaded => Count >= Capacity;

    /// <summary>
    /// Кладёт задачу. Ждёт, если достигнут жёсткий MaxCapacity
    /// (backpressure канала). Мягкий лимит не учитывается.
    /// </summary>
    public async ValueTask EnqueueAsync(string url, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(url, ct);
        RecalculateCapacity();
    }

    /// <summary>
    /// Кладёт задачу только если не превышен мягкий лимит.
    /// Возвращает false при перегрузке — продюсер сам решает:
    /// подождать, отбросить или залогировать.
    /// </summary>
    public async ValueTask<bool> TryEnqueueAsync(string url, CancellationToken ct = default)
    {
        if (IsOverloaded)
            return false;

        await _queue.Writer.WriteAsync(url, ct);
        RecalculateCapacity();
        return true;
    }

    public async ValueTask<string> DequeueAsync(CancellationToken ct = default)
    {
        var item = await _queue.Reader.ReadAsync(ct);
        RecalculateCapacity();
        return item;
    }

    /// <summary>
    /// Пересчитывает мягкий лимит по загруженности:
    /// растёт при заполнении ≥ 80 %, сжимается при ≤ 20 %.
    /// </summary>
    private void RecalculateCapacity()
    {
        var count = _queue.Reader.Count;
        if (count < 0) return;

        lock (_capacityLock)
        {
            if (count >= _capacity * 0.8 && _capacity < MaxCapacity)
            {
                _capacity = Math.Min(_capacity * 2, MaxCapacity);
            }
            else if (count <= _capacity * 0.2 && _capacity > MinCapacity)
            {
                _capacity = Math.Max(_capacity / 2, MinCapacity);
            }
        }
    }
}
