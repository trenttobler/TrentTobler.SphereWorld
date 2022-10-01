namespace TrentTobler.RetroCog.Geometry.Cuboid;

public class Cubitrix
{
    private const int TotalWords = 256 * 256 * 256 / 64;

    private int _count;
    private long[] _bits;

    public Cubitrix()
    {
        _bits = new long[TotalWords];
    }

    public int Count => _count;

    public override string ToString() => $"Cubicarray:{Count}";

    public bool this[Cubit3B cubit]
    {
        get
        {
            var word = (cubit.X << 10) | (cubit.Y << 2) | (cubit.Z >> 6);
            var bit = 1L << (cubit.Z & 63);
            var value = 0L != (_bits[word] & bit);
            return value;
        }
        set
        {
            var word = (cubit.X << 10) | (cubit.Y << 2) | (cubit.Z >> 6);
            var bit = 1L << (cubit.Z & 63);

            var bits = Interlocked.Read(ref _bits[word]);
            var orig = 0L != (bits & bit);
            if (orig == value)
                return;

            if (value)
            {
                if (bits == Interlocked.CompareExchange(ref _bits[word], bits | bit, bits))
                    Interlocked.Increment(ref _count);
            }
            else
            {
                if (bits == Interlocked.CompareExchange(ref _bits[word], bits & ~bit, bits))
                    Interlocked.Decrement(ref _count);
            }
        }
    }

    public IReadOnlyCollection<Face3B> OrientedFaces(Cubit3B cubit)
    {
        var result = new List<Face3B>(6);
        var center = HasCube(cubit);

        foreach (var face in cubit.Faces(true))
        {
            var faceCubes = face.Cubes();
            var other = this[faceCubes.Skip(1).First()];
            if (other == center)
                continue;
            result.Add(center ? face : face.Reversed);
        }

        return result;
    }

    public IReadOnlyCollection<Face3B> Faces(Cubit3B cubit)
        => cubit.Faces()
            .Where(HasFace)
            .ToArray();

    public bool HasFace(Face3B face)
        => 1 == face.Cubes().Where(HasCube).Count();

    public bool HasCube(Cubit3B cube)
        => this[cube];

    public IReadOnlyCollection<Vertex3B> Vertices((byte X, byte Y, byte Z) cubit)
        => Faces(cubit)
            .SelectMany(face => face.Vertices())
            .Distinct()
            .ToArray();

    public IReadOnlyCollection<Edge3B> Edges((byte X, byte Y, byte Z) cubit)
        => Faces(cubit)
            .SelectMany(face => face.NonOrientedEdges())
            .Distinct()
            .ToArray();

    public IEnumerable<Cubit3B> Cubes()
    {
        for (var index = _bits.Length; index-- > 0;)
        {
            var bits = Interlocked.Read(ref _bits[index]);
            var code = index << 6;
            for (var i = 0; i < 64 && bits != 0; ++i, bits >>= 1, ++code)
                if (0 != (bits & 1))
                    yield return new(code);
        }
    }

    public static T[] Reorient<T>(T[] items, bool flag)
    {
        if (flag)
            Array.Reverse(items);
        return items;
    }


    public record Summary(
        int CubitCount,
        int FaceCount,
        int EdgeCount,
        int VertexCount,
        Cubit3B MinCubit,
        Cubit3B MaxCubit)
    {
        public int EulerCharacteristic => VertexCount - EdgeCount + FaceCount;
        public Cubit3B Size => (MinCubit, MaxCubit).Size();
    }

    public Summary GetSummary()
    {
        var cubits = Cubes().ToArray();

        var faces = cubits
            .SelectMany(OrientedFaces)
            .Distinct()
            .ToArray();

        var edges = faces
            .SelectMany(face => face.NonOrientedEdges())
            .Select(edge => edge.NonOriented)
            .Distinct()
            .ToArray();

        var vertices = edges
            .SelectMany(edge => edge.Vertices())
            .Distinct()
            .ToArray();

        var minMax = cubits.MinMax();
        var summary = new Summary(
            Cubes()
                .Count(),
            Cubes()
                .SelectMany(OrientedFaces)
                .Count(),
            Cubes()
                .SelectMany(OrientedFaces)
                .SelectMany(face => face.OrientedEdges())
                .Where(edge => !edge.IsReversed)
                .Count(),
            Cubes()
                .SelectMany(OrientedFaces)
                .SelectMany(face => face.Vertices())
                .Distinct()
                .Count(),
            minMax.min,
            minMax.max);

        return summary;
    }
}
