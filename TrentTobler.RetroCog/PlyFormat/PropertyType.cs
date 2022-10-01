namespace TrentTobler.RetroCog.PlyFormat;

public enum PropertyType : byte
{
    Char,
    UChar,
    Short,
    UShort,
    Int,
    UInt,
    Float,
    Double,
}

public static class PropertyTypeExtensions
{
    private record PropertyTypeEntry(PropertyType PropertyType, string Name, double Min, double Max, int ByteCount);
    private static readonly PropertyTypeEntry[] Entries = new PropertyTypeEntry[]
    {
            //   type                 | name      | min               | max               | len
            new (PropertyType.Char,     "char",     sbyte.MinValue,     sbyte.MaxValue,     1),
            new (PropertyType.UChar,    "uchar",    byte.MinValue,      byte.MaxValue,      1),
            new (PropertyType.Short,    "short",    short.MinValue,     short.MaxValue,     2),
            new (PropertyType.UShort,   "ushort",   ushort.MinValue,    ushort.MaxValue,    2),
            new (PropertyType.Int,      "int",      int.MinValue,       int.MaxValue,       4),
            new (PropertyType.UInt,     "uint",     uint.MinValue,      uint.MaxValue,      4),
            new (PropertyType.Float,    "float",    float.MinValue,     float.MaxValue,     4),
            new (PropertyType.Double,   "double",   double.MinValue,    double.MaxValue,    8),
    };

    static PropertyTypeExtensions()
    {
        foreach(PropertyType propertyType in Enum.GetValues(typeof(PropertyType)))
        {
            var here = Entries[(int)propertyType];
            var there = Entries[(int)here.PropertyType];
            Entries[(int)here.PropertyType] = here;
            Entries[(int)there.PropertyType] = there;
        }
    }

    public static string ToTokenString(this PropertyType type)
        => Entries[(int)type].Name;

    public static PropertyType? ParseType(string? type)
    {
        if (type == null)
            return null;
        foreach (var entry in Entries)
            if (StringComparer.OrdinalIgnoreCase.Equals(entry.Name, type))
                return entry.PropertyType;
        return null;
    }

    public static double MinValue(this PropertyType type)
        => Entries[(int)type].Min;

    public static double MaxValue(this PropertyType type)
        => Entries[(int)type].Max;

    public static int ByteCount(this PropertyType type)
        => Entries[(int)type].ByteCount;

    public static double AsDouble(this PropertyType propertyType, byte[] binaryData, int offset)
    {
        var span = binaryData.AsSpan(offset);

        var value = propertyType switch
        {
            PropertyType.Char => (sbyte)span[0],
            PropertyType.UChar => span[0],
            PropertyType.Short => BitConverter.ToInt16(span),
            PropertyType.UShort => BitConverter.ToUInt16(span),
            PropertyType.Int => BitConverter.ToInt32(span),
            PropertyType.UInt => BitConverter.ToUInt32(span),
            PropertyType.Float => BitConverter.ToSingle(span),
            PropertyType.Double => BitConverter.ToDouble(span),

            _ => throw new NotImplementedException($"Header ValueType {propertyType} not implemented"),
        };

        return value;
    }

    private static bool TryWriteByte(Span<byte> span, byte data)
    {
        if (span.Length <= 0)
            return false;
        span[0] = data;
        return true;
    }

    public static bool TryWrite(this PropertyType propertyType, byte[] binaryData, int offset, double value)
    {
        var span = binaryData.AsSpan(offset);

        var success = propertyType switch
        {
            PropertyType.Char => TryWriteByte(span, (byte)(sbyte)value),
            PropertyType.UChar => TryWriteByte(span, (byte)value),
            PropertyType.Short => BitConverter.TryWriteBytes(span, (short)value),
            PropertyType.UShort => BitConverter.TryWriteBytes(span, (ushort)value),
            PropertyType.Int => BitConverter.TryWriteBytes(span, (int)value),
            PropertyType.UInt => BitConverter.TryWriteBytes(span, (uint)value),
            PropertyType.Float => BitConverter.TryWriteBytes(span, (float)value),
            PropertyType.Double => BitConverter.TryWriteBytes(span, value),

            _ => throw new NotImplementedException($"Header ValueType {propertyType} not implemented"),
        };

        return success;
    }

    public static int AsInt(this PropertyType propertyType, byte[] binaryData, int offset)
    {
        var span = binaryData.AsSpan(offset);

        var value = propertyType switch
        {
            PropertyType.Char => (sbyte)span[0],
            PropertyType.UChar => span[0],
            PropertyType.Short => BitConverter.ToInt16(span),
            PropertyType.UShort => BitConverter.ToUInt16(span),
            PropertyType.Int => BitConverter.ToInt32(span),
            PropertyType.UInt => (int)BitConverter.ToUInt32(span),
            PropertyType.Float => (int)BitConverter.ToSingle(span),
            PropertyType.Double => (int)BitConverter.ToDouble(span),

            _ => throw new NotImplementedException($"Header ValueType {propertyType} not implemented"),
        };

        return value;
    }
}
