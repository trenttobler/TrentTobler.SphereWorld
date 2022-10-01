using System.Collections;

namespace TrentTobler.RetroCog.PlyFormat;

public interface IDataProperty<T> : IReadOnlyList<T>
{
    IDataProperty<T> AsDouble();
    new T this[int index] { get; set; }
}

public record struct DataProperty(
    Header Header,
    HeaderElement HeaderElement,
    HeaderProperty HeaderProperty,
    byte[] BinaryData,
    int Offset
) : IDataProperty<double>
{
    private int GetDataOffset(int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException();

        if (HeaderProperty.ListLength == null)
            return index == 0 ? Offset : throw new IndexOutOfRangeException();

        var listLength = HeaderProperty.ListLength.Value;
        var maxIndex = listLength.AsInt(BinaryData, Offset);
        if (index < 0 || maxIndex <= index)
            throw new IndexOutOfRangeException();

        var itemOffset = Offset + listLength.ByteCount() + index * HeaderProperty.ValueType.ByteCount();

        return itemOffset;
    }

    public double this[int index]
    {
        get => HeaderProperty.ValueType.AsDouble(BinaryData, GetDataOffset(index));
        set => HeaderProperty.ValueType.TryWrite(BinaryData, GetDataOffset(index), value);
    }

    public int ByteLength
    {
        get
        {
            if (HeaderProperty.ListLength == null)
                return HeaderProperty.ValueType.ByteCount();

            var listLength = HeaderProperty.ListLength.Value;
            var maxIndex = listLength.AsInt(BinaryData, Offset);
            var listByteLength = listLength.ByteCount() + maxIndex * HeaderProperty.ValueType.ByteCount();

            return listByteLength;
        }
    }

    public void ApplyEndianReversal()
    {
        if (HeaderProperty.ListLength == null)
        {
            BinaryData.AsSpan(Offset, HeaderProperty.ValueType.ByteCount()).Reverse();
            return;
        }

        var listLength = HeaderProperty.ListLength.Value;
        BinaryData.AsSpan(Offset, listLength.ByteCount()).Reverse();

        var maxIndex = listLength.AsInt(BinaryData, Offset);
        var offset = Offset + listLength.ByteCount();
        var valueByteCount = HeaderProperty.ValueType.ByteCount();
        for (var i = 0; i < maxIndex; ++i)
        {
            BinaryData.AsSpan(offset, valueByteCount).Reverse();
            offset += valueByteCount;
        }
    }

    public int Count
    {
        get
        {
            if (HeaderProperty.ListLength == null)
                return 1;

            var listLength = HeaderProperty.ListLength.Value;
            var maxIndex = listLength.AsInt(BinaryData, Offset);

            return maxIndex;
        }
    }

    IDataProperty<double> IDataProperty<double>.AsDouble() => this;

    public IEnumerator<double> GetEnumerator()
    {
        var valueType = HeaderProperty.ValueType;
        var offset = Offset;
        if (HeaderProperty.ListLength == null)
        {
            yield return valueType.AsDouble(BinaryData, offset);
            yield break;
        }

        var listLength = HeaderProperty.ListLength.Value;
        var maxIndex = listLength.AsInt(BinaryData, Offset);
        offset += listLength.ByteCount();
        var valueByteCount = HeaderProperty.ValueType.ByteCount();
        for (var i = 0; i < maxIndex; ++i)
        {
            yield return valueType.AsDouble(BinaryData, offset);
            offset += valueByteCount;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

// the ply data model supports the following structure
//
//   element[0]
//      .item[0]
//          .property[0].item(Count = 1){ value }
//          .property[1].item(Count = 1){ value }
//          .property[2].item(Count = 1){ value }
//      .item[1]
//          .property[0].item(Count = 1){ value }
//          .property[1].item(Count = 1){ value }
//          .property[2].item(Count = 1){ value }
//      ...
//   element[1]
//      .item[0]
//          .property[0].list(Count = n){ value1, value2, value3, ... }
//          .property[1].item(Count = 1){ value }
//      .item[1]
//          .property[0].list(Count = n){ value1, value2, value3, ... }
//          .property[1].item(Count = 1){ value }
//      ...
//
//   ...

// we want to convert that to an OpenGL compatible data structure:
//  ArrayBuffer
//  ElementArrayBuffer
