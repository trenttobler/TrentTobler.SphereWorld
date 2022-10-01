using System.Collections;

namespace TrentTobler.RetroCog.Collections
{
    public class SpannableList<T>: IList<T>
    {
        private T[] _array = Array.Empty<T>();
        public int Count { get; private set; }

        public T this[int index]
        {
            get => _array.AsSpan(0, Count)[index];
            set => _array.AsSpan(0, Count)[index] = value;
        }

        public int Capacity
        {
            get => _array.Length;
            set
            {
                if (value < Count)
                    throw new ArgumentOutOfRangeException();
                if (value == Capacity)
                    return;
                Array.Resize(ref _array, value);
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        public SpannableList()
        {
        }

        public SpannableList(IEnumerable<T> items)
        {
            _array = items.ToArray();
            Count = _array.Length;
        }

        public int EnsureCapacity(int value)
            => value <= Capacity ? Capacity
                : (Capacity = Math.Max(Math.Max(value, 2 * Capacity), 16));

        public Span<T> AsSpan()
            => _array.AsSpan(0, Count);

        public Span<T> AsSpan(int start)
            => _array.AsSpan(0, Count)
                .Slice(start);

        public Span<T> AsSpan(int start, int count)
            => _array
                .AsSpan(0, Count)
                .Slice(start, count);

        public void Add(T item)
        {
            EnsureCapacity(Count + 1);
            _array[Count++] = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items is ICollection collection)
                EnsureCapacity(Count + collection.Count);

            foreach (var item in items)
                Add(item);
        }

        public void Clear()
        {
            Array.Fill(_array, default, 0, Count);
            Count = 0;
        }

        public bool Contains(T item)
            => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
            => _array.AsSpan(0, Count).TryCopyTo(array.AsSpan(arrayIndex));

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return _array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public int IndexOf(T item)
            => Array.IndexOf(_array, item, 0, Count);

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException();

            EnsureCapacity(Count + 1);
            Array.Copy(_array, index, _array, index + 1, Count - index);
            ++Count;
            _array[index] = item;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();
            Array.Copy(_array, index + 1, _array, index, Count - index - 1);
            _array[--Count] = default!;
        }
    }
}
