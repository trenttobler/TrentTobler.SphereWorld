using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TrentTobler.RetroCog.PlyFormat
{
    public class HeaderPropertyTest
    {
        public static IEnumerable<TestCaseData> ToStringTestData => new[]
        {
            new TestCaseData(string.Empty, HeaderProperty.Empty)
                .SetArgDisplayNames("empty"),

            new TestCaseData(string.Empty, new HeaderProperty("", PropertyType.Char))
                .SetArgDisplayNames("empty variation 1"),

            new TestCaseData(string.Empty, new HeaderProperty("", PropertyType.Int, PropertyType.UChar))
                .SetArgDisplayNames("empty variation 2"),

            new TestCaseData("property float x", new HeaderProperty("x", PropertyType.Float))
                .SetArgDisplayNames("simple float property"),

            new TestCaseData("property list uchar int vertex_index", new HeaderProperty("vertex_index", PropertyType.Int, PropertyType.UChar))
                .SetArgDisplayNames("list property"),
        };

        [TestCaseSource(nameof(ToStringTestData))]
        public void TestToString(string want, HeaderProperty property)
            => Assert.AreEqual(want, property.ToString());

        private static Regex ReSpace = new Regex(@"\s+", RegexOptions.Compiled);

        [TestCase(false, "")]
        [TestCase(false, "bad")]
        [TestCase(true, "property float x")]
        [TestCase(true, "property list uchar int vertex_index")]
        public void TestTryParse(bool want, string text)
        {
            var got = HeaderProperty.TryParse(text, out var property);
            Assert.AreEqual(want, got);

            Assert.AreEqual(got ? ReSpace.Replace(text, " ").Trim() : "", property.ToString());
        }
    }
}
