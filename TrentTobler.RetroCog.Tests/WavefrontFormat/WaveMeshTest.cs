using NUnit.Framework;
using System.IO;
using System.Linq;
using OpenTK.Mathematics;
using System.Collections.Generic;
using TrentTobler.RetroCog.Graphics;

namespace TrentTobler.RetroCog.WavefrontFormat;

public class WaveMeshTest
{
    public static IEnumerable<TestCaseData> ParseLinesSamples => new[]
    {
        new TestCaseData(Path.Combine("TestData", "cube.obj")),
    };

    [TestCaseSource(nameof(ParseLinesSamples))]
    public void TestParseLines(string filename)
    {
        using var reader = new StreamReader(filename);
        var mesh = WaveMeshParser.Parse(reader);
        CollectionAssert.AreEquivalent(new[]
        {
            new Vector3( +1, +1, -1),
            new Vector3( +1, -1, -1),
            new Vector3( +1, +1, +1),
            new Vector3( +1, -1, +1),
            new Vector3( -1, +1, -1),
            new Vector3( -1, -1, -1),
            new Vector3( -1, +1, +1),
            new Vector3( -1, -1, +1),
        }, mesh.Vertices.Select(x => x.Position).Distinct(), "Vertices");

        Assert.AreEqual(24, mesh.Edges.Count(), "Edges");
        Assert.AreEqual(6, mesh.Faces.Count, "Faces");
    }

}
