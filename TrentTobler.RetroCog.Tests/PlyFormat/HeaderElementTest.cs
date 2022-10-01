using NUnit.Framework;
using System.Collections.Generic;

namespace TrentTobler.RetroCog.PlyFormat
{
    public class HeaderElementTest
    {
        public static IEnumerable<TestCaseData> ToStringTestData => new[]
        {
            new TestCaseData(
                "element vertex 12\nproperty float X\nproperty float Y\nproperty float Z\n",
                new HeaderElement("vertex", 12)
                {
                    Properties =
                    {
                        new HeaderProperty("X", PropertyType.Float),
                        new HeaderProperty("Y", PropertyType.Float),
                        new HeaderProperty("Z", PropertyType.Float),
                    }
                }
            ).SetArgDisplayNames("vertices"),

            new TestCaseData(
                "element face 10\nproperty list uchar int vertex_list\n",
                new HeaderElement("face", 10)
                {
                    new HeaderProperty("vertex_list", PropertyType.Int, PropertyType.UChar),
                }
            ).SetArgDisplayNames("face"),
        };

        [TestCaseSource(nameof(ToStringTestData))]
        public void TestToString(string want, HeaderElement instance)
            => Assert.AreEqual(want, instance.ToString());
    }
}
