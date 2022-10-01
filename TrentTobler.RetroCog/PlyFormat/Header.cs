using System.Collections;
using System.Text;

namespace TrentTobler.RetroCog.PlyFormat;

public record Header(DataFormat Format, string Version = "1.0"): IEnumerable<HeaderElement>
{
    public List<HeaderElement> Elements { get; } = new List<HeaderElement>();

    public void Add(HeaderElement element)
        => Elements.Add(element);

    public IEnumerator<HeaderElement> GetEnumerator()
        => Elements.GetEnumerator();

    public override string ToString()
    {
        var sb = new StringBuilder("ply\n")
            .Append(FormattableString.Invariant($"format {Format.ToTokenString()} {Version}\n"));

        foreach (var element in Elements)
            sb.Append(element.ToString());

        return sb.Append("end_header\n")
            .ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
