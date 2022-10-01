using TrentTobler.RetroCog.Collections;

namespace TrentTobler.RetroCog.Geometry;

public class Mesh<T>
{
    public List<T> Vertices { get; } = new List<T>();
    public List<List<int>> Faces { get; } = new List<List<int>>();
    public IEnumerable<(int first, int second)> Edges => Faces.SelectMany(face => face.ToCyclicPairs());

    public Mesh<T> Clone()
    {
        var result = new Mesh<T>();
        result.Vertices.AddRange(Vertices);
        result.Faces.AddRange(Faces.Select(face => new List<int>(face)));
        return result;
    }

    public bool ValueEquals(Mesh<T> arg)
    {
        if (ReferenceEquals(this, arg))
            return true;

        if (Vertices.Count != arg.Vertices.Count
            || Faces.Count != arg.Faces.Count)
            return false;

        if (!Vertices.SequenceEqual(arg.Vertices))
            return false;

        if (Faces.Zip(arg.Faces, (lt, rt) => lt.SequenceEqual(rt)).All(x => x))
            return true;

        return false;
    }

    public (SpannableList<T> vertices, VertexIndexList elements) ToTriangulatedElements()
    {
        var vertices = new SpannableList<T>(Vertices);
        var elements = new VertexIndexList(Faces.SelectMany(face => face.Triangulate()).Select(n => (uint) n));
        return (vertices, elements);
    }

    public (SpannableList<T> vertices, VertexIndexList elements) ToOutlineElements()
    {
        var vertices = new SpannableList<T>(Vertices);
        var elements = new VertexIndexList(Faces.SelectMany(face => face.ToCyclicPairs().SelectMany(edge => new[] { edge.first, edge.second })).Select(n => (uint)n));
        return (vertices, elements);
    }
}
