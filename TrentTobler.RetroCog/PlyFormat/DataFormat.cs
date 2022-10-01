namespace TrentTobler.RetroCog.PlyFormat
{
    public enum DataFormat : byte
    {
        Ascii,
        BinaryBigEndian,
        BinaryLittleEndian,
    }

    public static class DataFormatExtensions
    {
        public static string ToTokenString(this DataFormat format)
            => format switch
            {
                DataFormat.Ascii => "ascii",
                DataFormat.BinaryBigEndian => "binary_big_endian",
                DataFormat.BinaryLittleEndian => "binary_little_endian",
                _ => throw new ArgumentException("invalid value", nameof(format)),
            };

        public static DataFormat? ParseFormat(string? text)
            => text switch
            {
                "ascii" => DataFormat.Ascii,
                "binary_big_endian" => DataFormat.BinaryBigEndian,
                "binary_little_endian" => DataFormat.BinaryLittleEndian,
                _ => null,
            };
    }
}
