using System.Text.RegularExpressions;

namespace TrentTobler.RetroCog.PlyFormat;

public record struct HeaderProperty(string Name, PropertyType ValueType, PropertyType? ListLength = null)
{
    public static HeaderProperty Empty { get; } = new HeaderProperty(string.Empty, default);

    public bool IsEmpty => string.IsNullOrEmpty(Name);
    public bool IsList => ListLength.HasValue;

    private static Dictionary<string, PropertyType> TypeLookup { get; }
        = Enum.GetValues(typeof(PropertyType))
            .Cast<PropertyType>()
            .ToDictionary(x => x.ToTokenString(), x => x, StringComparer.OrdinalIgnoreCase);

    private static string PropertyTypeNamesPattern { get; }
        = string.Join("|", TypeLookup.Keys);

    private static readonly Regex PropertyRegex = new(
        FormattableString.Invariant($@"property(\s+list\s+(?<len>{PropertyTypeNamesPattern}))?\s+(?<type>{PropertyTypeNamesPattern})\s+(?<name>\S+)"),
        RegexOptions.Compiled
        | RegexOptions.ExplicitCapture
        | RegexOptions.CultureInvariant);

    public override string ToString()
        => IsEmpty ? string.Empty
        : ListLength.HasValue ? FormattableString.Invariant($"property list {ListLength.Value.ToTokenString()} {ValueType.ToTokenString()} {Name}")
        : FormattableString.Invariant($"property {ValueType.ToTokenString()} {Name}");

    public static bool TryParse(string text, out HeaderProperty property)
    {
        property = Empty;

        var match = PropertyRegex.Match(text);
        if (!match.Success)
            return false;

        var typeGroup = match.Groups["type"];
        if (!TypeLookup.TryGetValue(typeGroup.Value, out var type))
            return false;

        var nameGroup = match.Groups["name"];
        if (!nameGroup.Success || string.IsNullOrWhiteSpace(nameGroup.Value))
            return false;

        var lenGroup = match.Groups["len"];
        PropertyType? len = null;
        if (lenGroup.Length > 0 && TypeLookup.TryGetValue(lenGroup.Value, out var lenValue))
            len = lenValue;

        var name = nameGroup.Value;

        property = new HeaderProperty(name, type, len);
        return true;
    }
}
