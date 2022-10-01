using NUnit.Framework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using TrentTobler.RetroCog.Geometry.Cuboid;

namespace TrentTobler.RetroCog.Geometry;

public class GeometryExtensionTests
{
    [TestCase("", "")]
    [TestCase("1", "")]
    [TestCase("1,2", "")]
    [TestCase("1,2,3", "1,2,3")]
    [TestCase("1,2,3,4", "1,2,3,1,3,4")]
    [TestCase("1,2,3,4,5", "1,2,3,1,3,4,1,4,5")]
    public void Triangulate_Should_MatchExpectations(string items, string want)
        => Assert.AreEqual(want, string.Join(",", items.Split(',', StringSplitOptions.RemoveEmptyEntries).Triangulate()));

    [Test]
    public void Triangulate_Should_HaveCorrectCount()
    {
        for (var verts = 0; verts < 50; ++verts)
        {
            var want = Math.Max(0, (verts - 2) * 3);
            var got = Enumerable.Range(0, verts).Triangulate().Count();
            Assert.AreEqual(want, got, $"{verts} vertices");
        }
    }

    [TestCase(101, 100)]
    public void TestToUnitSamples(int seed, int samples)
    {
        var rand = new Random(seed);
        for (var sample = 0; sample < samples; ++sample)
        {
            var vector = new Vector3(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
            var got = vector.FastUnit();
            Assert.AreEqual(1f, got.Length, 0.05);
        }

        Assert.AreEqual(Vector3.Zero, Vector3.Zero.FastUnit(), "Zero");
    }

    [Test]
    public void TestSumCount()
    {
        var (sum, count) = new[]
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 2, 0),
            new Vector3(0, 0, 3),
        }.SumCount();

        Assert.AreEqual(3, count, "count");
        Assert.AreEqual(new Vector3(1, 2, 3), sum, "sum"); 
    }

    public static IEnumerable<TestCaseData> VectorFromPointToTriangleSamples =>
        (
            from pw in new[]
            {
                (x: +0.00f, y: +0.00f, wx:+0.00f, wy: +0.00f),
                (x: +1.00f, y: +0.00f, wx:+0.00f, wy: +0.00f),
                (x: +0.00f, y: +1.00f, wx:+0.00f, wy: +0.00f),
                (x: +0.25f, y: +0.25f, wx:+0.00f, wy: +0.00f),
                (x: +2.00f, y: +0.00f, wx:+1.00f, wy: +0.00f),
                (x: +0.00f, y: +2.00f, wx:+0.00f, wy: +1.00f),
                (x: -1.00f, y: +0.00f, wx:-1.00f, wy: +0.00f),
                (x: +0.00f, y: -1.00f, wx:+0.00f, wy: -1.00f),
                (x: +2.00f, y: +2.00f, wx:+1.50f, wy: +1.50f),
                (x: -1.00f, y: +0.50f, wx:-1.00f, wy: +0.00f),
                (x: +0.50f, y: -1.00f, wx:+0.00f, wy: -1.00f),
                (x: -1.00f, y: +2.00f, wx:-1.00f, wy: +1.00f),
                (x: +2.00f, y: -1.00f, wx:+1.00f, wy: -1.00f),
                (x: +1.50f, y: -0.50f, wx:+0.50f, wy: -0.50f),
            }
            from z in new[] { 0f, 1f }
            from offset in new[] { new Vector3(0, 0, 0), new Vector3(1, 2, 3), }
            from matrix in new[]
            {
                new Matrix3(
                    1, 0, 0,
                    0, 1, 0,
                    0, 0, 1),
                new Matrix3(
                    0, 2, 0,
                    2, 0, 0,
                    0, 0, 2),
                new Matrix3(
                    0, 0, 3,
                    3, 0, 0,
                    0, 3, 0),
            }

            let p = matrix * (new Vector3(pw.x, pw.y, z) + offset)
            let a = matrix * (new Vector3(0, 0, 0) + offset)
            let b = matrix * (new Vector3(1, 0, 0) + offset)
            let c = matrix * (new Vector3(0, 1, 0) + offset)
            let w = matrix * new Vector3(pw.wx, pw.wy, z)

            select (p, a, b, c, w, id: $"variant")
        ).Concat(new[]
        {
            (
                p: new Vector3(+2.00f, -0.50f, +0.00f),
                a: new Vector3(+0.00f, +0.00f, +0.00f),
                b: new Vector3(+1.00f, +0.00f, +0.00f),
                c: new Vector3(+0.00f, +3.00f, +0.00f),
                w: new Vector3(+1.00f, -0.50f, +0.00f),
                id: "obtuse angle"
            ),
            (
                p: new Vector3(+9.00f, -0.25f, +0.00f),
                a: new Vector3(+0.00f, +0.00f, +0.00f),
                b: new Vector3(+4.00f, +0.00f, +0.00f),
                c: new Vector3(+0.00f, +1.00f, +0.00f),
                w: new Vector3(+5.00f, -0.25f, +0.00f),
                id: "acute angle"
            ),
        }
        ).SelectMany(d => new[]
        {
            (d.p, a: d.a, b: d.b, c: d.c, d.w, id: $"abc {d.id}"),
            (d.p, a: d.c, b: d.a, c: d.b, d.w, id: $"cab {d.id}"),
            (d.p, a: d.b, b: d.c, c: d.a, d.w, id: $"bca {d.id}"),
            (d.p, a: d.c, b: d.b, c: d.a, d.w, id: $"cba {d.id}"),
            (d.p, a: d.b, b: d.a, c: d.c, d.w, id: $"bac {d.id}"),
            (d.p, a: d.a, b: d.c, c: d.b, d.w, id: $"acb {d.id}"),
        })
        .Distinct()
        .Select((sample, n) => new TestCaseData(
            sample.p,
            sample.a, sample.b, sample.c,
            sample.w
        ));

    [TestCaseSource(nameof(VectorFromPointToTriangleSamples))]
    public void TestVectorFromPointToTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 want)
    {
        var got = p.VectorFromPointToTriangle(a, b, c);
        Assert.AreEqual(0.0, (want - got).Length, 1e-6, $"want {want}, but got {got}");
    }

    public static IEnumerable<TestCaseData> CubitIntersectsTriangles
        => new[]
        {
            (want: true, cubit: (0, 0, 0), a: (0.1f, 0.1f, 0.1f), b: (0.9f, 0.1f, 0.1f), c: (0.1f, 0.9f, 0.9f)),
            (want: true, cubit: (0, 0, 0), a: (1.0f, 0.0f, 0.5f), b: (1.0f, 2.0f, 0.5f), c: (2.0f, 2.0f, 0.5f)),
            (want: true, cubit: (0, 0, 0), a: (1.0f, 0.0f, 0.5f), b: (1.0f, 1.0f, 0.5f), c: (2.0f, 2.0f, 0.5f)),
            (want: true, cubit: (0, 0, 0), a: (1.0f, 0.0f, 0.0f), b: (1.0f, 2.0f, 0.0f), c: (2.0f, 2.0f, 0.0f)),
            (want: true, cubit: (0, 0, 0), a: (1.0f, 0.0f, 0.0f), b: (1.0f, 1.0f, 0.0f), c: (2.0f, 2.0f, 0.0f)),

            (want: false, cubit: (2, 2, 0), a: (0.0f, 0.0f, 0.5f), b: (1.0f, 0.0f, 0.5f), c: (0.0f, 1.0f, 0.5f)),
            (want: false, cubit: (0, 0, 0), a: (2.0f, 2.0f, 0.5f), b: (3.0f, 2.0f, 0.5f), c: (2.0f, 3.0f, 0.5f)),
            (want: false, cubit: (2, 2, 2), a: (2.0f, 2.0f, 0.5f), b: (3.0f, 2.0f, 0.5f), c: (2.0f, 3.0f, 0.5f)),
            (want: false, cubit: (2, 2, 2), a: (2.5f, 2.5f, 4.5f), b: (4.5f, 4.0f, 2.5f), c: (4.0f, 4.5f, 2.5f)),
            (want: false, cubit: (5, 5, 5), a: (4.5f, 4.5f, 2.5f), b: (1.5f, 1.0f, 4.5f), c: (1.0f, 1.5f, 4.5f)),
            (want: false, cubit: (2, 2, 2), a: (1.5f, 1.5f, 4.5f), b: (4.5f, 6.0f, 2.5f), c: (3.0f, 4.5f, 2.5f)),
            (want: false, cubit: (0, 0, 0), a: (1.2f, 0.9f, 0.9f), b: (0.9f, 1.2f, 0.9f), c: (1.9f, 1.9f, 2.0f)),
            (want: false, cubit: (2, 2, 2), a: (1.8f, 2.1f, 2.1f), b: (2.1f, 1.8f, 2.1f), c: (1.1f, 1.1f, 1.0f)),
        }
        .SelectMany(entry =>
        {
            var (want, cubit, a, b, c) = entry;
            return new[]
            {
                (want, cubit, a, b, c),
                (want, cubit, a, b: c, c: b),
                (want, cubit, a: c, b, c: a),
                (want, cubit, a: b, b: a, c),
                (want, cubit, a: b, b: c, c: a),
                (want, cubit, a: c, b: a, c: b),
            };
        })
        .SelectMany(entry =>
        {
            var (want, cubit, a, b, c) = entry;
            var (xx, yy, zz) = cubit;
            var (ax, ay, az) = entry.a;
            var (bx, by, bz) = entry.b;
            var (cx, cy, cz) = entry.c;

            return new[]
            {
                (want, cubit: (xx, yy, zz), a: (bx, by, bz), b: (cx, cy, cz), c: (ax, ay, az)),
                (want, cubit: (yy, zz, xx), a: (by, bz, bx), b: (cy, cz, cx), c: (ay, az, ax)),
                (want, cubit: (zz, xx, yy), a: (bz, bx, by), b: (cz, cx, cy), c: (az, ax, ay)),
                (want, cubit: (xx, zz, yy), a: (bx, bz, by), b: (cx, cz, cy), c: (ax, az, ay)),
                (want, cubit: (zz, yy, xx), a: (bz, by, bx), b: (cz, cy, cx), c: (az, ay, ax)),
                (want, cubit: (yy, xx, zz), a: (by, bx, bz), b: (cy, cx, cz), c: (ay, ax, az)),
            };
        })
        .Distinct()
        .Select(entry =>
        {
            var cubit = entry.cubit.ToCubit();
            var (ax, ay, az) = entry.a;
            var (bx, by, bz) = entry.b;
            var (cx, cy, cz) = entry.c;
            return new TestCaseData(
                entry.want,
                cubit,
                new Vector3(ax, ay, az),
                new Vector3(bx, by, bz),
                new Vector3(cx, cy, cz));
        });

    [TestCaseSource(nameof(CubitIntersectsTriangles))]
    public void TestCubitIntersectsTriangles(bool want, Cubit3B cubit, Vector3 a, Vector3 b, Vector3 c)
        => Assert.AreEqual(want, cubit.CubitIntersectsTriangle(a, b, c));
}
