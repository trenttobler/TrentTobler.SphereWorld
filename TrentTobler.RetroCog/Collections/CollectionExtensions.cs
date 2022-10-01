namespace TrentTobler.RetroCog.Collections;

public static class CollectionsExtensions
{
    public static T SelectAtRandom<T>(this IEnumerable<T> items, Random rand)
    {
        if (items is IReadOnlyList<T> list)
        {
            if (list.Count == 0)
                throw new ArgumentException("empty collection", nameof(items));
            var index = rand.Next(list.Count);
            return list[index];
        }

        using var iter = items.GetEnumerator();
        if (!iter.MoveNext())
            throw new ArgumentException("empty collection", nameof(items));

        var result = iter.Current;
        var count = 1;
        while (iter.MoveNext())
            if (rand.Next(++count) == 0)
                result = iter.Current;
        return result;
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> table, TKey key, Func<TValue> create)
    {
        if (table.TryGetValue(key, out var value))
            return value;
        value = create();
        table.Add(key, value);
        return value;
    }

    public static (TR first, TL second) Reverse<TL, TR>(this (TL first, TR second) entry)
        => (entry.second, entry.first);

    public static (T first, T second) Sort<T>(this (T first, T second) entry, IComparer<T>? comparer = null)
        => (comparer ?? Comparer<T>.Default).Compare(entry.first, entry.second) <= 0 ? (entry.first, entry.second)
            : (entry.second, entry.first);

    public static (TOut first, TOut second) Select<TIn, TOut>(this (TIn first, TIn second) entry, Func<TIn, TOut> f)
        => (f(entry.first), f(entry.second));

    public static IEnumerable<(T first, T second)> ToCyclicPairs<T>(this IEnumerable<T> items)
    {
        using var iter = items.GetEnumerator();
        if (!iter.MoveNext())
            yield break;

        var first = iter.Current;
        var prev = first;
        while (iter.MoveNext())
        {
            var next = iter.Current;
            yield return (prev, next);
            prev = next;
        }
        yield return (prev, first);
    }

    public static IEnumerable<(T first, T second, T third)> ToCyclicTriples<T>(this IEnumerable<T> items)
    {
        using var iter = items.GetEnumerator();
        if (!iter.MoveNext())
            yield break;

        var first = iter.Current;
        var last = first;

        if (!iter.MoveNext())
        {
            yield return (first, first, first);
            yield break;
        }

        var second = iter.Current;
        var here = second;

        while (iter.MoveNext())
        {
            var next = iter.Current;
            yield return (last, here, next);
            last = here;
            here = next;
        }

        yield return (last, here, first);
        yield return (here, first, second);
    }
}
