using NUnit.Framework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrentTobler.RetroCog.Geometry.Cuboid;

public class OrientableTests
{
    static IEnumerable<Cubit3B> RandomCubits(int? seed)
    {
        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        var samples = Enumerable.Repeat("gen", 50)
            .Select(_ => new Cubit3B((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next()))
            .Distinct()
            .ToArray();
        return samples;
    }

    static IEnumerable<(Cubit3B center, Cubit3B next)> RandomCubitPairs(int? seed)
    {
        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        var samples = Enumerable.Repeat("gen", 50)
            .Select(_ =>
            {
                var center = new Cubit3B((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next());
                var nextIndex = rand.Next(6);
                var next = center.FaceCubes().Skip(nextIndex).First();
                return (center, next);
            })
            .Distinct()
            .ToArray();
        return samples;
    }

    static IEnumerable<(Cubit3B cubit, Face3B face)> RandomCubitFaces(int? seed)
        => (
            from cubit in RandomCubits(seed)
            let faces = cubit.Faces(true)
            from face in faces
            select (cubit, face)
        ).Distinct();

    static IEnumerable<Face3B> RandomFaces(int? seed)
        => RandomCubitFaces(seed)
            .Select(data => data.face);

    [TestCaseSource(nameof(RandomCubitFaces), new object[] { 101 })]
    public void TestCubitOrientedFaceFirstCube((Cubit3B cubit, Face3B face) data)
        => Assert.AreEqual(data.cubit, data.face.Cubes().First());

    [TestCaseSource(nameof(RandomFaces), new object[] { 101 })]
    public void TestReversedFaceVertexOrder(Face3B face)
        => Assert.AreEqual(
            string.Join(", ", face.Reversed.Vertices().Reverse()),
            string.Join(", ", face.Vertices()));

    [TestCaseSource(nameof(RandomFaces), new object[] { 101 })]
    public void TestReversedFaceEdgeOrder(Face3B face)
        => Assert.AreEqual(
            string.Join(", ", face.Reversed.OrientedEdges().Select(edge => edge.Reversed).Reverse()),
            string.Join(", ", face.OrientedEdges()));

    [TestCaseSource(nameof(RandomFaces), new object[] { 101 })]
    public void TestFaceEdgeVertexOrder(Face3B face)
        => Assert.AreEqual(
            string.Join(", ", face.Reversed.OrientedEdges().Select(edge => edge.Reversed.Vertices().First()).Reverse()),
            string.Join(", ", face.OrientedEdges().Select(edge => edge.Vertices().First())));

    [TestCaseSource(nameof(RandomCubitPairs), new object[] { 101 })]
    public void TestCubitrixFaces((Cubit3B center, Cubit3B next) data)
    {
        var cubitrix = new Cubitrix();
        cubitrix[data.center] = true;
        cubitrix[data.next] = true;

        var inversed = new Cubitrix();
        foreach (var next in data.center.FaceCubes())
            if (next != data.next)
                inversed[next] = true;

        Assert.AreEqual(
            5,
            cubitrix.OrientedFaces(data.center).Count(),
            "Face Count");

        Assert.IsEmpty(
            cubitrix.OrientedFaces(data.center).Where(face => face.Cubes().First() != data.center),
            "First Cube of each oriented face should be center");

        Assert.AreEqual(
            string.Join(", ", inversed.OrientedFaces(data.center).Select(face => face.Reversed)),
            string.Join(", ", cubitrix.OrientedFaces(data.center)));
    }

    [Test]
    public void TestFaceNormals()
    {
        var cubit = new Cubit3B(1, 1, 1);
        var faces = cubit.Faces(true);
        foreach (var face in faces)
        {
            var verts =
                (
                    from v in face.Vertices()
                    let pos = v.Pos
                    let vec = new Vector3(pos.X, pos.Y, pos.Z)
                        - new Vector3(cubit.X, cubit.Y, cubit.Z)
                    select vec
                )
                .ToArray();

            var (ax, ay, az) = (verts[0][0], verts[0][1], verts[0][2]);
            var (bx, by, bz) = (verts[1][0], verts[1][1], verts[1][2]);
            var (cx, cy, cz) = (verts[2][0], verts[2][1], verts[2][2]);

            var (dx1, dy1, dz1) = (ax - bx, ay - by, az - bz);
            var (dx2, dy2, dz2) = (cx - bx, cy - by, cz - bz);

            var x = dy1 * dz2 - dz1 * dy2;
            var y = dz1 * dx2 - dx1 * dz2;
            var z = dx1 * dy2 - dy1 * dx2;

            var coords = new[] { x, y, z };
            var dir = coords.Where(n => n != 0).ToArray();
            Assert.AreEqual(1, dir.Length, $"{face}: {string.Join(", ", coords)}: should only have one nonzero normal coord");

            var got = new Cubit3B((byte)coords[0], (byte)coords[1], (byte)coords[2]);

            var (nx, ny, nz) = face.Cubes().Skip(1).First();
            var want = (1 - nx, 1 - ny, 1 - nz).ToCubit();

            Assert.AreEqual(got, want, $"{face}: normal");
        }
    }

    [TestCaseSource(nameof(RandomFaces), new object[] { 101 })]
    public void TestNonOrientedEdges(Face3B face)
        => Assert.AreEqual(
            string.Join(", ", face.NonOriented.OrientedEdges()),
            string.Join(", ", face.NonOrientedEdges()));
}
