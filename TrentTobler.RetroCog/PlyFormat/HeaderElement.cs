using System.Collections;
using System.Text;

namespace TrentTobler.RetroCog.PlyFormat
{
    public record HeaderElement(string Name, int Count) : IEnumerable<HeaderProperty>
    {
        public List<HeaderProperty> Properties { get; } = new List<HeaderProperty>();

        public void Add(HeaderProperty property)
            => Properties.Add(property);

        public IEnumerator<HeaderProperty> GetEnumerator()
            => Properties.GetEnumerator();

        public override string ToString()
        {
            var sb = new StringBuilder(FormattableString.Invariant($"element {Name} {Count}\n"));
            foreach (var property in Properties)
                sb.Append(property.ToString()).Append("\n");
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
