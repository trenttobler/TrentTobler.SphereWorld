using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrentTobler.RetroCog.PlyFormat
{
    public class HeaderTest
    {
        public static IEnumerable<TestCaseData> ToStringTestData => new[]
        {
            new TestCaseData(
                string.Join("\n",
                    "ply",
                    "format ascii 1.0",
                    "element vertex 8",
                    "property float x",
                    "property float y",
                    "property float z",
                    "element face 6",
                    "property list uchar int vertex_index",
                    "end_header",
                    ""),
                new Header(DataFormat.Ascii)
                {
                    Elements =
                    {
                        new HeaderElement("vertex", 8)
                        {
                            Properties =
                            {
                                new HeaderProperty("x", PropertyType.Float),
                                new HeaderProperty("y", PropertyType.Float),
                                new HeaderProperty("z", PropertyType.Float),
                            },
                        },
                        new HeaderElement("face", 6)
                        {
                            Properties =
                            {
                                new HeaderProperty("vertex_index", PropertyType.Int, PropertyType.UChar),
                            },
                        }
                    }
                })
            .SetArgDisplayNames("vertex,face sample"),

            new TestCaseData(
                string.Join("\n",
                    "ply",
                    "format ascii 1.0",
                    "element vertex 8",
                    "property float x",
                    "property float y",
                    "property float z",
                    "property uchar red",
                    "property uchar green",
                    "property uchar blue",
                    "element face 7",
                    "property list uchar int vertex_index",
                    "element edge 5",
                    "property int vertex1",
                    "property int vertex2",
                    "property uchar red",
                    "property uchar green",
                    "property uchar blue",
                    "end_header",
                    ""),
                new Header(DataFormat.Ascii)
                {
                    new HeaderElement("vertex", 8)
                    {
                        new HeaderProperty("x", PropertyType.Float),
                        new HeaderProperty("y", PropertyType.Float),
                        new HeaderProperty("z", PropertyType.Float),
                        new HeaderProperty("red", PropertyType.UChar),
                        new HeaderProperty("green", PropertyType.UChar),
                        new HeaderProperty("blue", PropertyType.UChar),
                    },
                    new HeaderElement("face", 7)
                    {
                        new HeaderProperty("vertex_index", PropertyType.Int, PropertyType.UChar),
                    },
                    new HeaderElement("edge", 5)
                    {
                        new HeaderProperty("vertex1", PropertyType.Int),
                        new HeaderProperty("vertex2", PropertyType.Int),
                        new HeaderProperty("red", PropertyType.UChar),
                        new HeaderProperty("green", PropertyType.UChar),
                        new HeaderProperty("blue", PropertyType.UChar),
                    },
                })
            .SetArgDisplayNames("vertex,face,edge sample"),
        };

        [TestCaseSource(nameof(ToStringTestData))]
        public void TestToString(string want, Header instance)
            => Assert.AreEqual(want, instance.ToString());
    }
}
