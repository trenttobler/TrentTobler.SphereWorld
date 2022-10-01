using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrentTobler.RetroCog.Geometry.Cuboid;

public class TopologyTests
{
    [Test]
    public void TestTopology()
    {
        var rand = new Random();
        Cubit3B[] samples = Enumerable.Repeat("gen", 50)
            .Select(_ => new Cubit3B((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next()))
            .ToArray();

        var mismatches = samples.SelectMany( cubit => new[]
        {
            from face in cubit.Faces()
            where !face.Cubes().Contains(cubit)
            select $"Missing cube->face->cube: {face} missing {cubit} in {string.Join(" ", face.Cubes())}",

            from edge in cubit.Edges()
            where !edge.Cubes().Contains(cubit)
            select $"Missing cube->edge->cube: {edge} missing {cubit} in {string.Join(" ", edge.Cubes())}",

            from vertex in cubit.Vertices()
            where !vertex.Cubes().Contains(cubit)
            select $"Missing cube->vertex->cube: {vertex} missing {cubit} in {string.Join(" ", vertex.Cubes())}",

            from face in cubit.Faces()
            from cube in face.Cubes()
            where !cube.Faces().Contains(face)
            select $"Missing face->cube->face: {cube} missing {face} in {string.Join(" ", cube.Faces())}",

            from face in cubit.Faces()
            from edge in face.OrientedEdges()
            where !edge.Faces().Contains(face)
            select $"Missing face->edge->face: {edge} missing {face} in {string.Join(" ", edge.Faces())}",

            from face in cubit.Faces()
            from vertex in face.Vertices()
            where !vertex.Faces().Contains(face)
            select $"Missing face->vertex->face: {vertex} missing {face} in {string.Join(" ", vertex.Faces())}",

            from edge in cubit.Edges()
            from cube in edge.Cubes()
            where !cube.Edges().Contains(edge)
            select $"Missing edge->cube->edge: {cube} missing {edge} in {string.Join(" ", cube.Edges())}",

            from edge in cubit.Edges()
            from face in edge.Faces()
            where !face.OrientedEdges().Contains(edge)
            select $"Missing edge->face->edge: {face} missing {edge} in {string.Join(" ", face.OrientedEdges())}",

            from edge in cubit.Edges()
            from vertex in edge.Vertices()
            where !vertex.Edges().Contains(edge)
            select $"Missing edge->vertex->edge: {vertex} missing {edge} in {string.Join(" ", vertex.Edges())}",

            from vertex in cubit.Vertices()
            from cube in vertex.Cubes()
            where !cube.Vertices().Contains(vertex)
            select $"Missing vertex->cube->vertex: {cube} missing {vertex} in {string.Join(" ", cube.Vertices())}",

            from vertex in cubit.Vertices()
            from face in vertex.Faces()
            where !face.Vertices().Contains(vertex)
            select $"Missing vertex->face->vertex: {face} missing {vertex} in {string.Join(" ", face.Vertices())}",

            from vertex in cubit.Vertices()
            from edge in vertex.Edges()
            where !edge.Vertices().Contains(vertex)
            select $"Missing vertex->edge->vertex: {edge} missing {vertex} in {string.Join(" ", edge.Vertices())}",

        }.SelectMany(s => s));

        Assert.IsEmpty(mismatches);
    }

    [Test]
    public void TestOrientedSequencing()
    {
        Cubit3B cubit = (0,0,0);

        foreach(var face in cubit.Faces())
        {
            var edges = face.OrientedEdges().ToList();
            edges.Add(edges.First());
            for(var i = 1; i < edges.Count; ++i)
            {
                Assert.AreEqual(
                    edges[i-1].Vertices().Except(edges[i].Vertices()).Count(),
                    1,
                    "adjacent edges should share a vertex");
            }

            var vertices = face.Vertices().ToList();
            vertices.Add(vertices.First());
            for(var i = 1; i < vertices.Count; ++i)
            {
                var edgeVerts = new[]{ vertices[i-1], vertices[i] };
                Assert.AreEqual(1, face.OrientedEdges()
                    .Count(edge => edge.Vertices().Intersect(edgeVerts).Count() == 2),
                    $"[{string.Join(",", edgeVerts)}] in {string.Join(", ", face.OrientedEdges())}");
            }
        }
    }
}
