using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using TrentTobler.RetroCog.Collections;
using TrentTobler.RetroCog.Geometry;
using TrentTobler.RetroCog.Geometry.Cuboid;

namespace TrentTobler.SphereWorld;

public class CuboidWorld
{
    private const int MaxSubdivisionLevels = 4;

    public int LevelOfDetail
    {
        get => _levelOfDetail;
        set => _levelOfDetail = Math.Max(0, Math.Min(MaxSubdivisionLevels, value));
    }
    private int _levelOfDetail = 2;

    public Mesh<Vertex> Mesh => EnsureLevelOfDetail(LevelOfDetail);

    public Cubitrix Cubitrix { get; }
    public List<Cubit3B> Strawberry { get; } = new List<Cubit3B>();
    public List<Cubit3B> Dirt { get; } = new List<Cubit3B>();

    private List<Mesh<Vertex>> MeshByLod { get; } = new();

    private CuboidWorld(Mesh<Vertex> mesh, Cubitrix cubitrix)
    {
        Cubitrix = cubitrix;
        MeshByLod.Add(mesh);
    }

    public (Vector3 pos, Vector3 norm) GetBestSurface(Vector3 pos, Vector3 norm)
    {
        // use the level 2 mesh to get the best surface sticky point.
        var mesh = EnsureLevelOfDetail(2);
        var facePoints = EnsureFacePoints(mesh);

        // search the points near the eye.

        var x = (int)Math.Round(pos.X);
        var y = (int)Math.Round(pos.Y);
        var z = (int)Math.Round(pos.Z);

        var nearest = facePoints[(x - 0, y - 0, z - 0)]
            .Concat(facePoints[(x - 1, y - 0, z - 0)])
            .Concat(facePoints[(x - 0, y - 1, z - 0)])
            .Concat(facePoints[(x - 1, y - 1, z - 0)])
            .Concat(facePoints[(x - 0, y - 0, z - 1)])
            .Concat(facePoints[(x - 1, y - 0, z - 1)])
            .Concat(facePoints[(x - 0, y - 1, z - 1)])
            .Concat(facePoints[(x - 1, y - 1, z - 1)])
            .Select(vert =>
            {
                var squared = (vert.Position - pos).LengthSquared;
                if (squared >= 1f)
                    return (weight: 0f, vert);

                var weight = (1 - squared) / (1 + squared);
                weight *= weight; // ^2
                weight *= weight; // ^4
                weight *= weight; // ^8
                return (weight, vert);
            })
            .Where(entry => entry.weight > 0f)
            .ToArray();

        var (
            totalWeight,
            totalPos,
            totalNorm
        ) = nearest
            .Aggregate(
                (totalWeight: 0f, totalPos: Vector3.Zero, totalNorm: Vector3.Zero),
                (sum, entry) => (
                    totalWeight: sum.totalWeight + entry.weight,
                    totalPos: sum.totalPos + entry.vert.Position * entry.weight,
                    totalNorm: sum.totalNorm + entry.vert.Normal * entry.weight
                ));

        if (totalWeight <= 1e-6)
            return (pos, norm);

        var surfaceNorm = totalNorm.FastUnit();
        var surfacePos = pos - Vector3.Dot(pos - totalPos / totalWeight, surfaceNorm) * surfaceNorm;

        return (surfacePos, surfaceNorm);
    }

    private ILookup<(int, int, int), Vertex>? _facePoints = null;
    private ILookup<(int, int, int), Vertex> EnsureFacePoints(Mesh<Vertex> mesh)
        => _facePoints ??= mesh.Faces
            .Select(face =>
            {
                Vector3 pos = Vector3.Zero;
                Vector2 tex = Vector2.Zero;
                Vector3 norm = Vector3.Zero;
                foreach (var v in face)
                {
                    pos += mesh.Vertices[v].Position;
                    tex += mesh.Vertices[v].TexCoord;
                    norm += mesh.Vertices[v].Normal;
                }

                pos /= face.Count;
                tex /= face.Count;
                norm.Normalize();

                var vertex = new Vertex(pos, tex, norm);
                var key = (
                    (int)Math.Floor(pos.X),
                    (int)Math.Floor(pos.Y),
                    (int)Math.Floor(pos.Z)
                );

                return (key, vertex);
            })
            .ToLookup(entry => entry.key, entry => entry.vertex);

    public static CuboidWorld GenerateWorld(int limit)
    {
        var growth = new CubitGrowth(101);
        for (var step = 0; ; step++)
            if (growth.Cubitrix.Count >= limit || !growth.Grow())
                break;

        var rand = new Random(101);

        var strawberry = growth.Cubitrix
            .Cubes()
            .SelectAtRandom(rand);

        var dirt = growth.Cubitrix
            .Cubes()
            .Where(x => x != strawberry)
            .SelectAtRandom(rand);

        var texCoords = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        Vector2 TexCoord(int n, int xtex, int ytex)
            => new Vector2(
                xtex * 128f / 512f + 1f / 512f + 126f / 512f * (((n + 1) >> 1) & 1),
                ytex * 128f / 512f + 1f / 512f + 126f / 512f * ((n >> 1) & 1));

        var faceTex = new Dictionary<Face3B, Vector2[]>();
        Vector2 FaceTex(Face3B face, int n)
        {
            if (!faceTex.TryGetValue(face, out var vecs))
            {
                var xtex = rand.Next(4);
                var ytex = rand.Next(2);

                if (face.Cubes().Any(x => x == strawberry))
                {
                    ytex += 2;
                    xtex &= 1;
                    xtex += 2;
                }
                else if (face.Cubes().Any(x => x == dirt))
                {
                    ytex += 2;
                    xtex &= 1;
                }

                var rot = rand.Next(4) + 4;
                var drot = rand.Next(2) * 2 - 1;
                vecs = new Vector2[]
                {
                    TexCoord(rot, xtex, ytex),
                    TexCoord(rot + drot, xtex, ytex),
                    TexCoord(rot + 2 * drot, xtex, ytex),
                    TexCoord(rot + 3 * drot, xtex, ytex),
                };
                faceTex.Add(face, vecs);
            }
            return vecs[n & 3];
        }

        var mesh = growth.Cubitrix
            .Cubes()
            .SelectMany(growth.Cubitrix.OrientedFaces)
            .ToMesh((face, vertex, n) => new Vertex(
                new(vertex.X, vertex.Y, vertex.Z),
                FaceTex(face, n),
                new(face.Normal.NX, face.Normal.NY, face.Normal.NZ)
            ));

        mesh.SmoothNormals(
            vert => vert.Position,
            vert => vert.Normal,
            (vert, normal) => new Vertex(vert.Position, vert.TexCoord, normal)
        );

        var world = new CuboidWorld(mesh, growth.Cubitrix);
        world.Strawberry.Add(strawberry);
        world.Dirt.Add(dirt);
        return world;
    }

    private Mesh<Vertex> EnsureLevelOfDetail(int limit)
    {
        while (MeshByLod.Count <= limit)
        {
            var last = MeshByLod.Last();
            var mesh = GenerateNextLevelOfDetail(last);
            MeshByLod.Add(mesh);
        }

        return MeshByLod[limit];
    }

    private static Mesh<Vertex> GenerateNextLevelOfDetail(Mesh<Vertex> last)
    {
        return last.CatmullClark(
            verts =>
            {
                var pos = Vector3.Zero;
                var tex = Vector2.Zero;
                var norm = Vector3.Zero;
                foreach (var vert in verts)
                {
                    pos += vert.Position;
                    tex += vert.TexCoord;
                    norm += vert.Normal;
                }
                pos /= verts.Length;
                tex /= verts.Length;
                var nlen = norm.Length;
                norm = nlen > 1e-6f ? norm / nlen : norm;
                return new Vertex(pos, tex, norm);
            },
            vert => vert.Position,
            (vert, pos) => new Vertex(pos, vert.TexCoord, vert.Normal)
        );
    }
}
