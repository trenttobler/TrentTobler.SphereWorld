using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace TrentTobler.RetroCog.PlyFormat
{
    public static class PlyConverter
    {
        private static Dictionary<Type, (object formatAscii, object formatBinary)> FormatByType { get; } = new Dictionary<Type, (object, object)>();

        static PlyConverter()
        {
            Func<T, char, string> Integer<T>()
                => (value, eol) => FormattableString.Invariant($"{value}{eol}");

            Func<T, char, string> Floating<T>()
                => (value, eol) => FormattableString.Invariant($"{value:R}{eol}");

            void AddInteger<T>(Func<T, byte[]> binary)
                => FormatByType.Add(typeof(T), (Integer<T>(), binary));

            void AddFloating<T>(Func<T, byte[]> binary)
                => FormatByType.Add(typeof(T), (Floating<T>(), binary));

            AddInteger<byte>(value => new byte[] { value });
            AddInteger<sbyte>(value => new byte[] { (byte)value });
            AddInteger<short>(BitConverter.GetBytes);
            AddInteger<ushort>(BitConverter.GetBytes);
            AddInteger<int>(BitConverter.GetBytes);
            AddInteger<uint>(BitConverter.GetBytes);
            AddFloating<float>(BitConverter.GetBytes);
            AddFloating<double>(BitConverter.GetBytes);
        }

        private static string FormatAscii<T>(T value, char eol)
            => ((Func<T, char, string>)FormatByType[typeof(T)].formatAscii)(value, eol);

        private static byte[] FormatBinary<T>(T value) => ((Func<T, byte[]>)FormatByType[typeof(T)].formatBinary)(value);

        private static byte[] GetBinary<T>(DataFormat format, T value)
        {
            var data = FormatBinary(value);
            if (BitConverter.IsLittleEndian == (format == DataFormat.BinaryLittleEndian))
                return data;
            Array.Reverse(data);
            return data;
        }

        public static byte[] ToBytes<T>(DataFormat format, T value, bool eol)
        {
            if (format == DataFormat.Ascii)
                return Encoding.ASCII.GetBytes(FormatAscii(value, eol ? '\n' : ' '));
            return GetBinary(format, value);
        }
    }

    public class Parser
    {
        public abstract record Token
        {
            public abstract IReadOnlyCollection<byte> ToBytes(DataFormat format);
        }

        public abstract record HeaderToken : Token
        {
            private HeaderBytes? _bytes;
            public override IReadOnlyCollection<byte> ToBytes(DataFormat format)
                => _bytes ??= new HeaderBytes(ToString());
        }

        public sealed record PlyToken : HeaderToken
        {
            public override string ToString() => "ply";
        }

        public sealed record CommentToken(string Comment) : HeaderToken
        {
            public override string ToString()
                => "comment " + Comment;
        }

        public sealed record FormatToken(DataFormat Format = DataFormat.BinaryLittleEndian, string Version = "1.0") : HeaderToken
        {
            public override string ToString()
                => FormattableString.Invariant(Format switch
                {
                    DataFormat.Ascii => $"format ascii {Version}",
                    DataFormat.BinaryBigEndian => $"format binary_little_endian {Version}",
                    DataFormat.BinaryLittleEndian => $"format binary_big_endian {Version}",
                    _ => throw new InvalidOperationException($"Invalid format {Format}"),
                });
        }

        public sealed record ElementToken(string Name, int Count) : HeaderToken
        {
            public override string ToString()
                => FormattableString.Invariant($"element {Name} {Count}");
        }

        public sealed record PropertyToken(string Name, PropertyType Item, PropertyType? List = null) : HeaderToken
        {
            public override string ToString()
                => FormattableString.Invariant(List switch
                {
                    PropertyType listIndex => $"property list {listIndex.ToTokenString()} {Item.ToTokenString()} {Name}",
                    null => $"property {Item.ToTokenString()} {Name}",
                });
        }

        public sealed record EndHeaderToken : HeaderToken
        {
            public override string ToString()
                => "end_header";
        }

        public enum DataTokenContext
        {
            Item,
            ListCount,
            ListElement,
        }

        public record DataToken(PropertyType PropertyType, double DoubleValue, DataTokenContext Context, bool Eol) : Token
        {
            public override string ToString()
            {
                var sb = new StringBuilder(20);

                switch (PropertyType)
                {
                    case PropertyType.Double:
                        sb.Append(FormattableString.Invariant($"{DoubleValue:R}"));
                        break;

                    case PropertyType.Float:
                        sb.Append(FormattableString.Invariant($"{(float)DoubleValue:R}"));
                        break;

                    default:
                        long integerVal = (long)DoubleValue;
                        sb.Append(FormattableString.Invariant($"{integerVal}"));
                        break;
                };

                sb.Append(Eol ? "\n" : " ");
                return sb.ToString();
            }

            public override IReadOnlyCollection<byte> ToBytes(DataFormat format)
            {
                switch (format)
                {
                    case DataFormat.Ascii:
                        return Encoding.ASCII.GetBytes(ToString());

                    case DataFormat.BinaryLittleEndian:
                    case DataFormat.BinaryBigEndian:
                        var result = new byte[PropertyType.ByteCount()];
                        PropertyType.TryWrite(result, 0, DoubleValue);

                        var formatIsLittleEndian = format == DataFormat.BinaryLittleEndian;
                        if (formatIsLittleEndian != BitConverter.IsLittleEndian)
                            Array.Reverse(result);

                        return result;
                }

                throw new ArgumentException($"Invalid format: {format}", nameof(format));
            }
        }

        private sealed record HeaderBytes(string Text) : IReadOnlyCollection<byte>
        {
            public int Count => Text.Length + 1;

            public IEnumerator<byte> GetEnumerator()
                => Text
                    .Select(c => (byte)c).Append((byte)'\n')
                    .GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        private class State
        {
            private Action<byte> HandleByte { get; set; }
            private StringBuilder Line { get; } = new StringBuilder();

            // NOTE: I'd like to turn these into more concrete types.  It'll do for now though.
            private List<string> Header { get; } = new List<string>();
            private List<byte> Data { get; } = new List<byte>();

            public State()
            {
                HandleByte = HandleHeaderByte;
            }

            private void HandleHeaderError(string line)
            {
                // TODO: handle errors in the header.
            }

            private void HandleHeaderLine(string line)
            {
                var command = ReWord.Match(line);
                switch (command.Value)
                {
                    case "ply":
                        // TODO:
                        break;

                    case "format":
                        // TODO:
                        break;

                    case "element":
                        // TODO:
                        break;

                    case "property":
                        // TODO:
                        break;

                    case "comment":
                        // TODO:
                        break;

                    case "end_header":
                        HandleByte = HandleDataByte;
                        break;

                    default:
                        HandleHeaderError(line);
                        break;
                }
            }

            private void HandleHeaderByte(byte b)
            {
                if (b == '\n')
                {
                    var line = Line.ToString();
                    Line.Clear();
                    HandleHeaderLine(line);
                }
            }

            private void HandleDataByte(byte b)
            {
                Data.Add(b);
            }

            private void HandleEnd()
            {
            }
        }

        private static Regex ReWord = new Regex(
            @"\S+",
            RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.ExplicitCapture);

        private IEnumerable<(int number, string[] args)> HeaderLines(IEnumerator<byte> bytes)
        {
            var sb = new StringBuilder();
            var args = new List<string>();
            var lineNumber = 1;
            var column = 1;

            while (bytes.MoveNext())
            {
                if (bytes.Current == '\n')
                {
                    if (sb.Length > 0)
                        args.Add(sb.ToString());
                    sb.Clear();

                    yield return (lineNumber, args.ToArray());

                    if (args.Count == 1 && args[0] == "end_header")
                        yield break;

                    args.Clear();
                    ++lineNumber;
                    column = 1;
                    continue;
                }

                var ch = (char)bytes.Current;
                ++column;

                if (char.IsWhiteSpace(ch))
                {
                    if (sb.Length > 0)
                        args.Add(sb.ToString());
                    continue;
                }

                sb.Append(ch);
            }

            throw InvalidOperation(lineNumber, "unexpected end of file");
        }

        private DataToken ReadDataToken(IEnumerator<byte> bytes, DataFormat format, PropertyType type, DataTokenContext context, bool eol)
            => format == DataFormat.Ascii ? ReadAsciiDataToken(bytes, type, context, eol)
            : ReadBinaryDataToken(bytes, format, type, context, eol);

        private DataToken ReadBinaryDataToken(IEnumerator<byte> bytes, DataFormat format, PropertyType type, DataTokenContext context, bool eol)
        {
            var len = type.ByteCount();
            var binary = new byte[len];
            for (var i = 0; i < len; ++i)
            {
                if (!bytes.MoveNext())
                    throw new InvalidOperationException("unexpected end of data while reading token");
                binary[i] = bytes.Current;
            }

            var doubleValue = type.AsDouble(binary, 0);

            return new DataToken(type, doubleValue, context, eol);
        }

        private static DataToken ReadAsciiDataToken(IEnumerator<byte> bytes, PropertyType type, DataTokenContext context, bool eol)
        {
            var sb = new StringBuilder();
            while (bytes.MoveNext())
            {
                var ch = (char)bytes.Current;
                if (!char.IsWhiteSpace(ch))
                    sb.Append(ch);
                else if (sb.Length > 0)
                    break;
            }

            if (sb.Length == 0)
                throw new InvalidOperationException("unexpected end of data while reading token");

            var doubleValue = double.Parse(sb.ToString());

            return new DataToken(type, doubleValue, context, eol);
        }

        private static Exception InvalidOperation(int lineNumber, string message)
            => new InvalidOperationException($"Line {lineNumber}: {message}");

        private static Dictionary<string, Func<int, int, Func<int, string?>, Token>> GenTokens = new()
        {
            ["ply"] = (line, n, args) => new PlyToken(),

            ["comment"] = (line, n, args) => new CommentToken(string.Join(" ", Enumerable.Range(1, n).Select(args))),

            ["format"] = (line, n, args) =>
            {
                var format = DataFormatExtensions.ParseFormat(args(1));
                var version = args(2);
                if (format == null || version == null)
                    throw InvalidOperation(line, "invalid format line");
                return new FormatToken(format.Value, version);
            },

            ["element"] = (line, n, args) =>
            {
                var name = args(1);
                var count = int.TryParse(args(2), out var intVal) ? intVal : -1;
                if (name == null || count < 0)
                    throw InvalidOperation(line, "invalid element line");
                return new ElementToken(name, count);
            },

            ["property"] = (line, n, args) =>
            {
                var (isList, name, item, list) = args(1) == "list" ?
                    (
                        true,
                        args(4),
                        PropertyTypeExtensions.ParseType(args(3)),
                        PropertyTypeExtensions.ParseType(args(2))
                    )
                    :
                    (
                        false,
                        args(2),
                        PropertyTypeExtensions.ParseType(args(1)),
                        null
                    );

                if (name == null || (isList && list == null) || item == null)
                    throw InvalidOperation(line, "invalid property line");

                return new PropertyToken(name, item.Value, list);
            },

            ["end_header"] = (line, n, args) => new EndHeaderToken(),
        };

        public IEnumerable<Token> Tokenize(IEnumerator<byte> bytes)
        {
            Header header = new Header(DataFormat.Ascii);
            List<HeaderElement> elements = new List<HeaderElement>();

            foreach (var line in HeaderLines(bytes))
            {
                if (line.args.Length == 0)
                    continue;

                if (!GenTokens.TryGetValue(line.args[0], out var genToken))
                    throw InvalidOperation(line.number, $"invalid line command: {line.args[0]}");

                var token = genToken(line.number, line.args.Length, n => n >= 0 && n < line.args.Length ? line.args[n] : null);
                yield return token;

                static void NoOp() { }

                if (token is PlyToken || token is CommentToken)
                    NoOp();
                else if (token is FormatToken formatToken)
                    header = new Header(formatToken.Format, formatToken.Version);
                else if (token is ElementToken elementToken)
                    elements.Add(new HeaderElement(elementToken.Name, elementToken.Count));
                else if (token is PropertyToken propertyToken)
                    elements.Last().Properties.Add(new HeaderProperty(propertyToken.Name, propertyToken.Item, propertyToken.List));
                else if (token is EndHeaderToken)
                    break;
            }

            foreach (var element in elements)
            {
                for (var i = 0; i < element.Count; i++)
                {
                    for (var p = 0; p < element.Properties.Count; p++)
                    {
                        var property = element.Properties[p];
                        if (property.ListLength is PropertyType indexType)
                        {
                            var listLength = ReadDataToken(bytes, header.Format, indexType, DataTokenContext.ListCount, false);
                            yield return listLength;

                            for (var listItem = 0; listItem < listLength.DoubleValue; listItem++)
                            {
                                var listValue = ReadDataToken(bytes, header.Format, property.ValueType, DataTokenContext.ListElement, false);
                                yield return listValue;
                            }
                        }
                        else
                        {
                            var dataValue = ReadDataToken(bytes, header.Format, property.ValueType, DataTokenContext.Item, false);
                            yield return dataValue;
                        }
                    }
                }
            }
        }

        public void Parse(IEnumerator<byte> data)
        {
            DataFormat? format = null;
            string version = string.Empty;
            List<HeaderElement> elements = new();

            foreach (var token in Tokenize(data))
            {
                if (token is CommentToken
                    || token is PlyToken)
                    continue;

                if (token is FormatToken formatToken)
                {
                    format = formatToken.Format;
                    version = formatToken.Version;
                    continue;
                }

                if (token is ElementToken elementToken)
                {
                    var element = new HeaderElement(elementToken.Name, elementToken.Count);
                    elements.Add(element);
                    continue;
                }

                if (token is PropertyToken propertyToken)
                {
                    var property = new HeaderProperty(propertyToken.Name, propertyToken.Item, propertyToken.List);
                    elements.Last().Add(property);
                    continue;
                }

                throw new NotImplementedException($"TODO: implement token type {token.GetType().Name}");
            }

            if (format == null)
                throw new InvalidOperationException("Header is missing format");

            var header = new Header(format.Value, version);

            byte NextByte()
            {
                if (format == DataFormat.Ascii)
                    throw new NotImplementedException("TODO: implement ascii nextByte");
                if (data.MoveNext())
                    return data.Current;
                throw new InvalidOperationException("unexpected end of data");
            }

            byte[] NextBytes(int len)
            {
                if (format == DataFormat.Ascii)
                    throw new NotImplementedException("TODO: implement ascii nextBytes");

                var result = new byte[len];
                for(var i = 0; i < len; ++i)
                    result[i] = NextByte();
                if (BitConverter.IsLittleEndian != (format == DataFormat.BinaryLittleEndian))
                    Array.Reverse(result);

                return result;
            }
        }
    }
}
