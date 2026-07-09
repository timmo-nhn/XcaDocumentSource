namespace XcaXds.Commons.Commons;

public class BoundedDictionary<TKey, TValue> where TKey : notnull
{
    private readonly int _maxSize;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _dict;
    private readonly LinkedList<(TKey Key, TValue Value)> _order;
    public event EventHandler<BoundedDictionaryItemAddedEventArgs<TKey, TValue>>? Updated;

    public BoundedDictionary(int maxSize = 1000)
    {
        if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize));
        _maxSize = maxSize;
        _dict = [];
        _order = new LinkedList<(TKey, TValue)>();
    }

    public void Add(TKey key, TValue value)
    {
        using var mutex = new Mutex(false, "Global\\AddMutex");
        var mutexAcquired = false;
        BoundedDictionaryItemAddedEventArgs<TKey, TValue>? eventArgs = null;

        try
        {
            mutexAcquired = mutex.WaitOne(TimeSpan.FromSeconds(5));
            if (mutexAcquired)
            {
                var node = new LinkedListNode<(TKey, TValue)>((key, value));
                _order.AddLast(node);
                _dict[key] = node;

                if (_dict.Count > _maxSize)
                {
                    var oldest = _order.First!;
                    _order.RemoveFirst();
                    _dict.Remove(oldest.Value.Key);
                }

                eventArgs = new BoundedDictionaryItemAddedEventArgs<TKey, TValue>(key, value);
            }
        }
        finally
        {
            if (mutexAcquired)
            {
                mutex.ReleaseMutex();
            }
        }

        if (eventArgs != null)
        {
            Updated?.Invoke(this, eventArgs);
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_dict.TryGetValue(key, out var node))
        {
            value = node.Value.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> Items =>
        _order.Select(n => new KeyValuePair<TKey, TValue>(n.Key, n.Value));
}

public sealed class BoundedDictionaryItemAddedEventArgs<TKey, TValue>(TKey key, TValue value) : EventArgs
{
    public TKey Key { get; } = key;
    public TValue Value { get; } = value;
    public KeyValuePair<TKey, TValue> Item => new(Key, Value);
}