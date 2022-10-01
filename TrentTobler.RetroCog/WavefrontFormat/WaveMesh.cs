using OpenTK.Mathematics;
using TrentTobler.RetroCog.Geometry;

namespace TrentTobler.RetroCog.WavefrontFormat;

public static class WaveMeshParser
{
    public delegate T VertexMap<T>(Vector4 pos, Vector3? tex, Vector3? norm);

    public static Mesh<Vertex> Parse(TextReader reader)
    {
        Mesh<Vertex> mesh = new();
        Dictionary<Vertex, int> vertexIndices = new();
        List<Vector4> vs = new();
        List<Vector3> vts = new();
        List<Vector3> vns = new();

        int EnsureVertexIndex(Vertex vertex)
        {
            if (vertexIndices.TryGetValue(vertex, out var index))
                return index;
            index = mesh.Vertices.Count;
            mesh.Vertices.Add(vertex);
            vertexIndices.Add(vertex, index);
            return index;
        }

        TOut? ElementItem<TOut>(List<TOut> list, int index) where TOut : struct
            => index == 0 ? null
            : index < -list.Count ? null
            : index > list.Count ? null
            : index > 0 ? list[index - 1]
            : list[list.Count + index];

        int? ToMeshVertex(string faceElement)
        {
            var parts = faceElement.Split('/');
            var v = ElementItem(vs, ParseInt(parts, 0) ?? 0);
            if (v == null)
                return null;
            var vt = ElementItem(vts, ParseInt(parts, 1) ?? 0);
            var vn = ElementItem(vns, ParseInt(parts, 2) ?? 0);
            var vertex = new Vertex(
                v.Value.Xyz / v.Value.W,
                vt?.Xy ?? default,
                vn ?? default
            );
            return EnsureVertexIndex(vertex);
        }

        List<int> ToMeshFace(IEnumerable<string> faceElements)
            => new(
                from faceElement in faceElements
                let index = ToMeshVertex(faceElement)
                where index != null
                select index.Value);

        var lines = reader
            .ToLines()
            .CombineBackslashedLines()
            .Select(s => s.TrimHashComment().SplitWords())
            .Where(args => args.Any());

        foreach (var line in lines)
        {
            switch (line[0])
            {
                case "v":
                    vs.Add(new Vector4(
                        ParseFloat(line, 1) ?? 0,
                        ParseFloat(line, 2) ?? 0,
                        ParseFloat(line, 3) ?? 0,
                        ParseFloat(line, 4) ?? 1));
                    break;

                case "vt":
                    vts.Add(new Vector3(
                        ParseFloat(line, 1) ?? 0,
                        ParseFloat(line, 2) ?? 0,
                        ParseFloat(line, 3) ?? 0));
                    break;

                case "vn":
                    vns.Add(new Vector3(
                        ParseFloat(line, 1) ?? 0,
                        ParseFloat(line, 2) ?? 0,
                        ParseFloat(line, 3) ?? 0));
                    break;

                case "f":
                    mesh.Faces.Add(ToMeshFace(line.Skip(1)));
                    break;

            }
        }

        return mesh;
    }

    private static float? ParseFloat(IReadOnlyList<string> args, int index)
    => args.Count > index && float.TryParse(args[index], out var value) ? value : null;

    private static int? ParseInt(IReadOnlyList<string> args, int index)
        => args.Count > index && int.TryParse(args[index], out var value) ? value : null;
}
