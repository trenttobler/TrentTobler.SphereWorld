namespace TrentTobler.RetroCog.Geometry.Cuboid;

public class CubitGrowth
{
    private Random Rand { get; }
    private List<Cubit3B> Boundary { get; } = new();

    public Cubitrix Cubitrix { get; } = new Cubitrix();

    public CubitGrowth(int seed)
    {
        Rand = new Random(seed);
        Boundary.Add((127, 127, 127));
    }

    public bool Grow()
    {
        if (Boundary.Count == 0)
            return false;

        var nextCube = RemoveNextCube();
        if (Cubitrix[nextCube]
            || GetBadCandidateVertices(nextCube).Any())
        {
            return true;
        }

        Cubitrix[nextCube] = true;
        ExpandCubitBoundary(nextCube);

        return true;
    }

    private void ExpandCubitBoundary(Cubit3B nextCube)
    {
        var candidates = nextCube
            .FaceCubes()
            .Where(cube => !Cubitrix[cube]);

        Boundary.AddRange(candidates);
    }

    private IEnumerable<Vertex3B> GetBadCandidateVertices(Cubit3B nextCube)
        =>
            from vertex in nextCube.Vertices()
            let cubeTest = CandidateCubeTest(nextCube)
            let faceCount = GetCandidateFaces(cubeTest, vertex).Count()
            where faceCount > 0 && (faceCount < 3 || faceCount > 5)
            select vertex;

    private static IEnumerable<Face3B> GetCandidateFaces(Func<Cubit3B, bool> cubeTest, Vertex3B vertex)
        =>
            from face in vertex.Faces()
            let isBoundaryFace = face.Cubes().Where(cubeTest).Count() == 1
            where isBoundaryFace
            select face;

    private Func<Cubit3B, bool> CandidateCubeTest(Cubit3B nextCube)
        => cube
        => (cube == nextCube) ? !Cubitrix[cube] // toggle result for next cube
            : Cubitrix[cube]; // normal result for all other cubes

    private Cubit3B RemoveNextCube()
    {
        var index = Rand.Next(Boundary.Count);
        var nextCube = Boundary[index];

        Boundary[index] = Boundary.Last();
        Boundary.RemoveAt(Boundary.Count - 1);

        return nextCube;
    }
}
