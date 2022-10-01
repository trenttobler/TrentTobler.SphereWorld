using OpenTK.Mathematics;
using TrentTobler.RetroCog.Collections;
using TrentTobler.RetroCog.Geometry.Cuboid;

namespace TrentTobler.RetroCog.Geometry;

public delegate T AverageVertices<T>(params T[] vertices);

public static class GeometryExtensions
{
    public static IEnumerable<(T a, T b, T c)> ToTriangles<T>(this IEnumerable<T> vertices)
    {
        using var vertex = vertices.GetEnumerator();

        if (!vertex.MoveNext())
            yield break;
        var first = vertex.Current;

        if (!vertex.MoveNext())
            yield break;
        var second = vertex.Current;

        if (!vertex.MoveNext())
            yield break;
        var current = vertex.Current;

        yield return (first, second, current);
        while (vertex.MoveNext())
        {
            var next = vertex.Current;
            yield return (first, current, next);
            current = next;
        }
    }

    public static IEnumerable<T> Triangulate<T>(this IEnumerable<T> vertices)
        => vertices.ToTriangles()
            .SelectMany(tri => new[] { tri.a, tri.b, tri.c });

    public static Vector3 FastUnit(this Vector3 direction)
    {
        var length = direction.LengthFast;
        if (length < 1e-6)
            return Vector3.Zero;
        return direction / length;
    }

    public static (Vector3 sum, int cnt) SumCount(this IEnumerable<Vector3> vectors)
        => vectors.Aggregate(
            (sum: Vector3.Zero, cnt: 0),
            (cur, v) => (
                sum: cur.sum + v,
                cnt: cur.cnt + 1));

    public static Vector3 Average(this IEnumerable<Vector3> vectors)
        => vectors.SumCount() switch
        {
            (Vector3 zero, 0) => zero,
            (Vector3 one, 1) => one,
            (Vector3 sum, int cnt) => sum / cnt,
        };

    public static void SmoothNormals<T>(
        this Mesh<T> mesh,
        Func<T, Vector3> getPos,
        Func<T, Vector3> getNorm,
        Func<T, Vector3, T> setNorm)
    {
        var vertexByPosition = mesh.Vertices
            .Select((vertex, index) => (vertex, index))
            .GroupBy(entry => getPos(entry.vertex), entry => entry.index);

        foreach (var entry in vertexByPosition)
        {
            var normal = (entry
                .Aggregate(
                    Vector3.Zero,
                    (sum, index) => sum + getNorm(mesh.Vertices[index]))
                / entry.Count()).FastUnit();

            foreach (var index in entry)
                mesh.Vertices[index] = setNorm(mesh.Vertices[index], normal);
        }
    }

    private static (Vector3[] positions, int[] vertexToPosIndex) DistinctVertexPositions<T>(
        IReadOnlyList<T> vertices,
        Func<T, Vector3> getPos)
    {
        var indexByPosition = new Dictionary<Vector3, int>();
        var positions = new Vector3[vertices.Count];
        var vertexToPosIndex = new int[vertices.Count];
        for (var i = 0; i < vertices.Count; ++i)
        {
            var vertex = getPos(vertices[i]);
            if (indexByPosition.TryGetValue(vertex, out var index))
            {
                vertexToPosIndex[i] = index;
                continue;
            }

            positions[i] = vertex;
            vertexToPosIndex[i] = i;
            indexByPosition.Add(vertex, i);
        }

        return (positions, vertexToPosIndex);
    }

    public static Mesh<T> CatmullClark<T>(
        this Mesh<T> mesh,
        AverageVertices<T> blend,
        Func<T, Vector3> getPos,
        Func<T, Vector3, T> setPos)
        where T: notnull
    {
        var submesh = new Mesh<T>();
        var vertices = submesh.Vertices.AsIndexedList(5 * mesh.Vertices.Count, EqualityComparer<T>.Default);

        var (positions, vertexToPosIndex) = DistinctVertexPositions(mesh.Vertices, getPos);

        // For each face, add a face point
        //  * Set each face point to be the average of all original points for the respective face
        Vector3 toFacePoint(List<int> face) => face.Count switch
        {
            0 => Vector3.Zero,
            1 => positions[vertexToPosIndex[face[0]]],
            int n => face
                .Select(v => vertexToPosIndex[v])
                .OrderBy(v => v)
                .Aggregate(Vector3.Zero, (sum, v) => sum + positions[v]) / n,
        };
        var facePoints = mesh.Faces.Select(toFacePoint).ToArray();

        var facesByPosIndex = mesh.Faces
            .SelectMany((face, faceIndex) => face
                .Select(vertex => (posIndex: vertexToPosIndex[vertex], faceIndex)))
            .ToLookup(entry => entry.posIndex, entry => entry.faceIndex);

        // For each edge, add an edge point.
        //  * Set each edge point to be the average of the two neighbouring face points(AF) and the midpoint of the edge(ME)
        //
        //           AF + ME
        //          ---------
        //              2
        //
        var toPosEdge = ((int first, int second) edge) => (vertexToPosIndex[edge.first], vertexToPosIndex[edge.second]).Sort();

        var facesByPosEdge = mesh.Faces
            .SelectMany((face, faceIndex) => face
                .ToCyclicPairs()
                .Select(edge => (posEdge: toPosEdge(edge), faceIndex)))
            .ToLookup(entry => entry.posEdge, entry => entry.faceIndex);

        T EdgePoint((int first, int second) edge)
        {
            var (firstV, secondV) = edge
                .Select(v => mesh.Vertices[v]);

            var (posSum, posCnt) = facesByPosEdge[toPosEdge(edge)]
                .Select(faceIndex => facePoints[faceIndex])
                .SumCount();

            var edgePoint = setPos(
                blend(firstV, secondV),
                (getPos(firstV) + getPos(secondV) + posSum) / (posCnt + 2));

            return edgePoint;
        }

        // For each original point (P),
        //      take the average (F) of all n (recently created) face points for faces touching P,
        //      and take the average (R) of all n edge midpoints for original edges touching P,
        //      where each edge midpoint is the average of its two endpoint vertices (not to be
        //      confused with new edge points above).
        //      (Note that from the perspective of a vertex P, the number of edges neighboring P
        //          is also the number of adjacent faces, hence n)
        //
        //  * Move each original point to the new vertex point
        //
        //           F + 2R + (n-3)P
        //          -----------------           (This is the barycenter of P, R and F with respective weights(n − 3), 2 and 1)
        //                  n
        //
        var vertexPosEdges = mesh.Faces
            // convert every face into a list of edges
            .SelectMany(face => face
                .ToCyclicPairs()
                .Select(toPosEdge))
            .SelectMany(edge =>
            {
                var forward = (vertex: edge.first, edge);
                var backward = (vertex: edge.second, edge);
                // Special case when an edge starts and ends at the same vertex.
                return edge.first == edge.second ? new[] { forward }
                    : new[] { forward, backward };
            })
            .ToLookup(entry => entry.vertex, entry => entry.edge);

        var vertexPoint = (int vertexIndex) =>
        {
            var p = positions[vertexToPosIndex[vertexIndex]];

            var (fSum, n) = facesByPosIndex[vertexToPosIndex[vertexIndex]]
                .Select(faceIndex => facePoints[faceIndex])
                .SumCount();
            var f = fSum / n;

            var (sumR, cntR) = vertexPosEdges[vertexToPosIndex[vertexIndex]].Aggregate(
                (sum: Vector3.Zero, cnt: 0),
                (cur, posEdge) => (
                    sum: cur.sum + positions[posEdge.first] + positions[posEdge.second],
                    cnt: cur.cnt + 2));
            var r = sumR / cntR;

            return (f + 2 * r + (n - 3) * p) / n;
        };

        IEnumerable<List<int>> SubFaces(List<int> face, int faceIndex)
        {
            var center = blend(face
                .Select(v => mesh.Vertices[v])
                .ToArray());

            var centerIndex = vertices.GetIndex(center);

            foreach (var (lastV, hereV, nextV) in face.ToCyclicTriples())
            {
                var here = setPos(mesh.Vertices[hereV], vertexPoint(hereV));

                var nextE = EdgePoint((hereV, nextV));
                var lastE = EdgePoint((lastV, hereV));

                var hereIndex = vertices.GetIndex(here);
                var nextIndex = vertices.GetIndex(nextE);
                var lastIndex = vertices.GetIndex(lastE);

                yield return new List<int>(4)
                {
                    hereIndex,
                    nextIndex,
                    centerIndex,
                    lastIndex
                };
            }
        }

        submesh.Faces.AddRange(mesh.Faces.SelectMany(SubFaces));
        return submesh;
    }

    public static Mesh<TOut> ToMesh<TIn, TOut>(
        this IEnumerable<IEnumerable<TIn>> faces,
        Func<TIn, TOut> toVertex,
        IEqualityComparer<TOut>? vertexEquality = null)
        where TOut: notnull
    {
        var mesh = new Mesh<TOut>();
        var indexByVertex = new Dictionary<TOut, int>(vertexEquality ?? EqualityComparer<TOut>.Default);
        int GetIndex(TIn vin)
        {
            var vout = toVertex(vin);
            if (indexByVertex.TryGetValue(vout, out var index))
                return index;

            index = mesh.Vertices.Count();
            mesh.Vertices.Add(vout);
            indexByVertex.Add(vout, index);

            return index;
        }

        mesh.Faces.AddRange(faces.Select(face => face.Select(GetIndex).ToList()));

        return mesh;
    }

    private static float ProjectAlong(Vector3 pos, Vector3 dir)
        => Vector3.Dot(pos, dir) / Vector3.Dot(dir, dir);

    public static Vector3 VectorFromPointToTriangle(
        this Vector3 p,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        //              c
        //              * 
        //             / \
        //           dac  dbc
        //           /     \
        //          a--dab--b
        //          |
        //          |normab

        var dap = p - a;
        var dab = b - a;
        var dac = c - a;
        var norm = Vector3.Cross(dab, dac);

        // check which side of line ab the point is on...
        var normab = Vector3.Cross(dab, norm);
        var pnormab = ProjectAlong(dap, normab);
        if (pnormab >= 0)
        {
            var pab = ProjectAlong(dap, dab);
            return pab <= 0 ? dap   // closest to point a.
                : pab >= 1 ? p - b  // closest to point b.
                : dap - pab * dab;  // closest to a point between a and b.
        }

        // check which side of line ac the point is on...
        var normac = Vector3.Cross(dac, norm);
        var pnormac = ProjectAlong(dap, normac);
        if (pnormac <= 0)
        {
            var pac = ProjectAlong(dap, dac);
            return pac <= 0 ? dap   // closest to point a.
                : pac >= 1 ? p - c  // closest to point c.
                : dap - pac * dac;  // closest to a point between a and c.
        }

        // check which side of line bc the point is on...
        var dbc = c - b;
        var dbp = p - b;
        var normbc = Vector3.Cross(dbc, norm);
        var pnormbc = ProjectAlong(dbp, normbc);
        if (pnormbc >= 0)
        {
            var pbc = ProjectAlong(dbp, dbc);
            return pbc <= 0 ? dbp   // closest to point b.
                : pbc >= 1 ? p - c  // closest to point c.
                : dbp - pbc * dbc;  // close to a point between a and b.
        }

        // point is inside, computed directly via a normal projection.
        return ProjectAlong(dap, norm) * norm;
    }

    public static Cubit3B ToCubit(this (int x, int y, int z) tuple)
        => new Cubit3B((byte)tuple.x, (byte)tuple.y, (byte)tuple.z);

    public static bool CubitIntersectsTriangle(this Cubit3B cubit, Vector3 a, Vector3 b, Vector3 c)
    {
        static (float min, float max) Project(IEnumerable<Vector3> points, Vector3 axis)
            => points
                .Select(p => Vector3.Dot(axis, p))
                .Aggregate(
                    (min: float.PositiveInfinity, max: float.NegativeInfinity),
                    (limit, dot) => (min: Math.Min(dot, limit.min), max: Math.Max(dot, limit.max)));

        static bool CheckNormal(float offset, IEnumerable<Vector3> points, Vector3 axis)
        {
            var dots = Project(points, axis);
            return dots.max < offset || dots.min > offset + 1;
        }

        // check box normals
        var triVerts = new[] { a, b, c };
        if (CheckNormal(cubit.X, triVerts, new Vector3(1, 0, 0))
            || CheckNormal(cubit.Y, triVerts, new Vector3(0, 1, 0))
            || CheckNormal(cubit.Z, triVerts, new Vector3(0, 0, 1)))
            return false;

        var boxVerts = new[]
        {
            new Vector3(cubit.X + 0, cubit.Y + 0, cubit.Z + 0),
            new Vector3(cubit.X + 1, cubit.Y + 0, cubit.Z + 0),
            new Vector3(cubit.X + 0, cubit.Y + 1, cubit.Z + 0),
            new Vector3(cubit.X + 1, cubit.Y + 1, cubit.Z + 0),
            new Vector3(cubit.X + 0, cubit.Y + 0, cubit.Z + 1),
            new Vector3(cubit.X + 1, cubit.Y + 0, cubit.Z + 1),
            new Vector3(cubit.X + 0, cubit.Y + 1, cubit.Z + 1),
            new Vector3(cubit.X + 1, cubit.Y + 1, cubit.Z + 1),
        };

        // check triangle normal
        var triNorm = Vector3.Cross(b - a, c - a);
        if (CheckNormal(Vector3.Dot(triNorm, a), boxVerts, triNorm))
            return false;

        // check edge cross products
        static bool CheckEdge(Vector3 edge, IEnumerable<Vector3> boxes, IEnumerable<Vector3> tris)
        {
            var box = Project(boxes, edge);
            var tri = Project(tris, edge);
            return box.max < tri.min || box.min > tri.max;
        }
        var ab = a - b;
        var bc = b - c;
        var ca = c - a;

        if (
            CheckEdge(new Vector3(0, ab.Z, -ab.Y), boxVerts, triVerts)
            || CheckEdge(new Vector3(-ab.Z, 0, ab.X), boxVerts, triVerts)
            || CheckEdge(new Vector3(ab.Y, -ab.X, 0), boxVerts, triVerts)

            || CheckEdge(new Vector3(0, bc.Z, -bc.Y), boxVerts, triVerts)
            || CheckEdge(new Vector3(-bc.Z, 0, bc.X), boxVerts, triVerts)
            || CheckEdge(new Vector3(bc.Y, -bc.X, 0), boxVerts, triVerts)

            || CheckEdge(new Vector3(0, ca.Z, -ca.Y), boxVerts, triVerts)
            || CheckEdge(new Vector3(-ca.Z, 0, ca.X), boxVerts, triVerts)
            || CheckEdge(new Vector3(ca.Y, -ca.X, 0), boxVerts, triVerts))

            return false;

        return true;
    }

    public static IEnumerable<Cubit3B> IntersectingTriangleCubits(
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        var min = Vector3.ComponentMin(a, b);
        var max = Vector3.ComponentMax(a, b);
        if (max.X - min.X > 2 
            || max.Y - min.Y > 2
            || max.Z - min.Z > 2)
            throw new NotImplementedException("Brute force is no longer appropriate for these triangles - need to make this better");

        var x0 = (int)Math.Floor(Math.Max(0, min.X));
        var y0 = (int)Math.Floor(Math.Max(0, min.Y));
        var z0 = (int)Math.Floor(Math.Max(0, min.Z));

        var x1 = (int)Math.Ceiling(Math.Min(255, max.X));
        var y1 = (int)Math.Ceiling(Math.Min(255, max.Y));
        var z1 = (int)Math.Ceiling(Math.Min(255, max.Z));

        for (var x = x0; x <= x1; ++x)
            for (var y = y0; y <= y1; ++y)
                for (var z = z0; z <= z1; ++z)
                {
                    var cubit = (x, y, z).ToCubit();
                    if (cubit.CubitIntersectsTriangle(a, b, c))
                        yield return cubit;
                }
    }
}
