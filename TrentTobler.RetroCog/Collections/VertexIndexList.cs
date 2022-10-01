using OpenTK.Graphics.OpenGL;
using System.Collections;

namespace TrentTobler.RetroCog.Collections;

/// <summary>
/// A list of OpenGL DrawElementType elements that adjusts type based on content (byte, ushort, uint).
/// </summary>
public class VertexIndexList : IList<uint>
{
    private const int _minSize = 32;
    private byte[]? _byteList = null;
    private ushort[]? _shortList = null;
    private uint[]? _intList = null;

    public int Count { get; private set; }
    public DrawElementsType ElementType { get; private set; } = DrawElementsType.UnsignedByte;

    public uint this[int index]
    {
        get => ElementType switch
        {
            DrawElementsType.UnsignedInt => _intList!.AsSpan(0, Count)[index],
            DrawElementsType.UnsignedShort => _shortList!.AsSpan(0, Count)[index],
            DrawElementsType.UnsignedByte => _byteList?.AsSpan(0, Count)[index] ?? throw new IndexOutOfRangeException(),
            _ => throw new InvalidOperationException($"{ElementType}: Invalid internal state"),
        };
        set
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            ApplyValue(value,
                (ref byte[] bytes, byte arg) => bytes[index] = arg,
                (ref ushort[] bytes, ushort arg) => bytes[index] = arg,
                (ref uint[] bytes, uint arg) => bytes[index] = arg);
        }
    }

    public VertexIndexList(IEnumerable<uint>? items = null)
    {
        if (items != null)
            AddRange(items);
    }

    public IEnumerator<uint> GetEnumerator()
        => (
            _intList
            ?? _shortList?.Select(x => (uint)x)
            ?? _byteList?.Select(x => (uint)x)
            ?? Enumerable.Empty<uint>()
        )
        .Take(Count)
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public bool TryByteSpan(out Span<byte> span)
    {
        if (Count == 0)
        {
            span = Array.Empty<byte>();
            return true;
        }

        if (_byteList == null)
        {
            span = default;
            return false;
        };

        span = _byteList.AsSpan(0, Count);
        return true;
    }

    public bool TryShortSpan(out Span<ushort> span)
    {
        if (_shortList == null)
        {
            span = default;
            return false;
        }
        span = _shortList.AsSpan(0, Count);
        return true;
    }

    public bool TryIntSpan(out Span<uint> span)
    {
        if (_intList == null)
        {
            span = default;
            return false;
        }
        span = _intList.AsSpan(0, Count);
        return true;
    }

    public void Add(uint value) => ApplyValue(
        value,
        (ref byte[] array, byte arg) => Append(ref array!, arg),
        (ref ushort[] array, ushort arg) => Append(ref array!, arg),
        (ref uint[] array, uint arg) => Append(ref array!, arg));

    public void AddRange(IEnumerable<uint> values)
    {
        foreach (var value in values)
            Add(value);
    }

    private int Append<T>(ref T[] array, T item)
    {
        if (Count >= array.Length)
            Array.Resize(ref array, Count * 2);

        array[Count] = item;
        return Count++;
    }

    private delegate TOut Command<TItem, TOut>(ref TItem[] array, TItem arg);

    private T ApplyValue<T>(
        uint value,
        Command<byte, T> withBytes,
        Command<ushort, T> withShorts,
        Command<uint, T> withInts)
    {
        switch (ElementType)
        {
            case DrawElementsType.UnsignedInt:
                return withInts(ref _intList!, value);

            case DrawElementsType.UnsignedShort:
                if (value > ushort.MaxValue)
                {
                    ConvertToUintArray();
                    return withInts(ref _intList!, value);
                }
                return withShorts(ref _shortList!, (ushort)value);

            case DrawElementsType.UnsignedByte:
                if (value > ushort.MaxValue)
                {
                    ConvertToUintArray();
                    return withInts(ref _intList!, value);
                }
                if (value > byte.MaxValue)
                {
                    ConvertToUshortArray();
                    return withShorts(ref _shortList!, (ushort)value);
                }
                _byteList ??= new byte[_minSize];
                return withBytes(ref _byteList, (byte)value);

            default:
                throw new InvalidOperationException($"{ElementType}: Invalid internal state");
        }
    }

    private void ConvertToUshortArray()
    {
        ElementType = DrawElementsType.UnsignedShort;

        _shortList = new ushort[LengthWhenConverting(_byteList?.Length)];

        if (_byteList != null)
        {
            for (var i = 0; i < Count; ++i)
                _shortList[i] = _byteList[i];
            _byteList = null;
            return;
        }
    }

    private int LengthWhenConverting(int? len)
    {
        if (len == null)
            return _minSize;
        var result = Count + _minSize / 2 >= len ? len.Value * 2
            : len.Value;
        return result;
    }

    private void ConvertToUintArray()
    {
        ElementType = DrawElementsType.UnsignedInt;
        _intList = new uint[LengthWhenConverting(_byteList?.Length ?? _shortList?.Length)];

        if (_shortList != null)
        {
            for (var i = 0; i < Count; ++i)
                _intList[i] = _shortList[i];
            _shortList = null;
            return;
        }

        if (_byteList != null)
        {
            for (var i = 0; i < Count; ++i)
                _intList[i] = _byteList[i];
            _byteList = null;
            return;
        }
    }

    bool ICollection<uint>.IsReadOnly => false;

    public void Clear()
        => Count = 0;

    public bool Contains(uint item)
        => IndexOf(item) >= 0;

    public void CopyTo(uint[] array, int arrayIndex)
    {
        foreach (var value in this)
            array[arrayIndex++] = value;
    }

    public bool Remove(uint item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;

        RemoveAt(index);
        return true;
    }

    public int IndexOf(uint item)
        => ElementType switch
        {
            DrawElementsType.UnsignedByte when item > byte.MaxValue || _byteList == null => -1,
            DrawElementsType.UnsignedByte => Array.IndexOf(_byteList, (byte)item, 0, Count),

            DrawElementsType.UnsignedShort when item > ushort.MaxValue => -1,
            DrawElementsType.UnsignedShort => Array.IndexOf(_shortList!, (ushort)item, 0, Count),

            DrawElementsType.UnsignedInt => Array.IndexOf(_intList!, item, 0, Count),

            _ => throw new InvalidOperationException($"{ElementType}: Invalid internal state"),
        };

    public void Insert(int index, uint item)
    {
        if (index < 0 || index > Count)
            throw new IndexOutOfRangeException();

        int InsertAt<T>(ref T[] data, T value)
        {
            ++Count;
            if (Count >= data.Length)
                Array.Resize(ref data, 2 * data.Length);

            Array.Copy(data, index, data, index + 1, Count - index);
            data[index] = value;
            return index;
        }

        ApplyValue(item, InsertAt, InsertAt, InsertAt);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException();

        int RemoveAt<T>(ref T[] data, T _)
        {
            Array.Copy(data, index + 1, data, index, --Count - index);
            return index;
        }

        ApplyValue(0, RemoveAt, RemoveAt, RemoveAt);
    }
}
