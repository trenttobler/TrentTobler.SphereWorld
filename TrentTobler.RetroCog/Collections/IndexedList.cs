using System.Collections;

namespace TrentTobler.RetroCog.Collections;

public class IndexedList<T> : IReadOnlyList<T>
    where T: notnull
{
    private IList<T> Items { get; }
    private Dictionary<T, int> ItemIndex { get; }

    public int Count => Items.Count;
    public T this[int index] => Items[index];

    public static IndexedList<T> Reindex(IList<T> items, int capacity, IEqualityComparer<T> comparer)
    {
        var itemIndex = new Dictionary<T, int>(Math.Min(capacity, items.Count), comparer);
        for (var i = 0; i < items.Count; ++i)
            itemIndex.TryAdd(items[i], i);
        return new(items, itemIndex);
    }

    private IndexedList(IList<T> items, Dictionary<T, int> itemIndex)
    {
        Items = items;
        ItemIndex = itemIndex;
    }

    public IndexedList(int capacity, IEqualityComparer<T> comparer)
    {
        Items = new List<T>(capacity);
        ItemIndex = new Dictionary<T, int>(capacity, comparer);
    }

    public IndexedList()
        : this(16, EqualityComparer<T>.Default)
    {
    }

    public int GetIndex(T item)
    {
        if (ItemIndex.TryGetValue(item, out var index))
            return index;

        index = Items.Count;
        ItemIndex.Add(item, index);
        Items.Add(item);
        return index;
    }

    public IEnumerable<int> GetIndices(IEnumerable<T> items)
        => items.Select(GetIndex);

    public IEnumerator<T> GetEnumerator()
        => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

public static class IndexedListExtensions
{
    public static IndexedList<T> AsIndexedList<T>(this IList<T> list, int capacity, IEqualityComparer<T> comparer)
        where T : notnull
        => IndexedList<T>.Reindex(list, capacity, comparer);
}