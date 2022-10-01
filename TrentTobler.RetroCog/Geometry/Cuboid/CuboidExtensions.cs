using OpenTK.Mathematics;

namespace TrentTobler.RetroCog.Geometry.Cuboid;

public static class CuboidExtensions
{
    public static (
        Vector3 origin,
        float scale
    ) ComputeOriginScale(this IEnumerable<Vertex3B> vertices)
    {
        var minMax = vertices.Select(v => v.Pos).MinMax();

        var origin = new Vector3(
            (minMax.max.X + minMax.min.X) * 0.5f,
            (minMax.max.Y + minMax.min.Y) * 0.5f,
            (minMax.max.Z + minMax.min.Z) * 0.5f
        );

        var scale = 2.0f / new[]
        {
            minMax.max.X - minMax.min.X,
            minMax.max.Y - minMax.min.Y,
            minMax.max.Z - minMax.min.Z,
        }.Max();

        return (origin, scale);
    }

    public static Mesh<T> ToMesh<T>(
        this IEnumerable<Face3B> faces,
        Func<Face3B, Vertex3B, int, T> createVertex)
        where T: notnull
    {
        var mesh = new Mesh<T>();

        var indexByVertex = new Dictionary<T, int>();
        int VertexIndex(T vert)
        {
            if (indexByVertex.TryGetValue(vert, out var index))
                return index;
            index = mesh.Vertices.Count;
            mesh.Vertices.Add(vert);
            indexByVertex.Add(vert, index);
            return index;
        }

        List<int> CreateFace(Face3B face)
            => face
                .Vertices()
                .Select((vert, n) => VertexIndex(createVertex(face, vert, n)))
                .ToList();

        mesh.Faces.AddRange(faces.Select(CreateFace));

        return mesh;
    }

    public static (Cubit3B min, Cubit3B max) MinMax(this IEnumerable<Cubit3B> cubits)
        => cubits.Aggregate(
            (min: Cubit3B.MaxValue, max: Cubit3B.MinValue),
            (cur, pos) => (min: Cubit3B.Min(cur.min, pos), max: Cubit3B.Max(cur.max, pos))
        );

    public static Cubit3B Size( this (Cubit3B min, Cubit3B max) cur)
        => new (
            (byte)(cur.max.X - cur.min.X),
            (byte)(cur.max.Y - cur.min.Y),
            (byte)(cur.max.Z - cur.min.Z)
        );
}
