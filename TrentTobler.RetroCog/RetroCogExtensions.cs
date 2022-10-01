using System.Text;
using System.Text.RegularExpressions;

namespace TrentTobler.RetroCog;

public static class RetroCogExtensions
{
    public static string TrimHashComment(this string line)
    {
        var hash = line.IndexOf('#');
        if (hash >= 0)
            return line.Substring(0, hash);

        return line;
    }

    public static IEnumerable<string> CombineBackslashedLines(this IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        using var iter = lines.GetEnumerator();
        while (iter.MoveNext())
        {
            var line = iter.Current;
            if (line.EndsWith('\\'))
            {
                sb.Append(line, 0, line.Length - 1);
            }
            else if (sb.Length == 0)
            {
                yield return line;
            }
            else
            {
                sb.Append(line);
                yield return sb.ToString();
                sb.Clear();
            }
        }

        if (sb.Length > 0)
            yield return sb.ToString();
    }

    public static IEnumerable<string> ToLines(this TextReader inp)
    {
        for (var line = inp.ReadLine(); line != null; line = inp.ReadLine())
            yield return line;
    }

    private static Regex ReWord = new Regex(@"\S+");

    public static IReadOnlyList<string> SplitWords(this string line)
        => ReWord.Matches(line).Select(x => x.Value).ToArray();

    public static float NextFloat(this Random rand)
        => (float)rand.NextDouble();

    public static string ToMetric(this double value)
    {
        if (value < 1e-3)
            return (value * 1e6).ToString("N0") + "n";
        if (value < 1)
            return (value * 1e3).ToString("N1") + "m";
        if (value < 1e3)
            return value.ToString("N1");
        if (value < 1e6)
            return (value * 1e-3).ToString("N1") + "k";
        if (value < 1e9)
            return (value * 1e-6).ToString("N1") + "M";
        return (value * 1e-9).ToString("N1") + "G";
    }
}
