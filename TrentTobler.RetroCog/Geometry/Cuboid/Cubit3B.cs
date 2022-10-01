namespace TrentTobler.RetroCog.Geometry.Cuboid;

public record struct Cubit3B(byte X, byte Y, byte Z)
{
    public (byte X, byte Y, byte Z) ToXYZ()
        => (X, Y, Z);

    public int ToCode()
        => (X << 16) | (Y << 8) | Z;

    public Cubit3B(int code)
        : this((byte)(code >> 16), (byte)(code >> 8), (byte)code)
    {
    }

    public Cubit3B AddX(int x) => new((byte)(X + x), Y, Z);
    public Cubit3B AddY(int y) => new(X, (byte)(Y + y), Z);
    public Cubit3B AddZ(int z) => new(X, Y, (byte)(Z + z));
    public Cubit3B AddXY(int x, int y) => new((byte)(X + x), (byte)(Y + y), Z);
    public Cubit3B AddYZ(int y, int z) => new(X, (byte)(Y + y), (byte)(Z + z));
    public Cubit3B AddXZ(int x, int z) => new((byte)(X + x), Y, (byte)(Z + z));
    public Cubit3B AddXYZ(int x, int y, int z) => new((byte)(X + x), (byte)(Y + y), (byte)(Z + z));

    public IReadOnlyCollection<Face3B> Faces(bool oriented = false) => new[]
    {
        Face3B.YBottom(this),
        Face3B.XLeft(this),
        Face3B.ZBack(this),
        Face3B.YBottom(AddY(1), oriented),
        Face3B.XLeft(AddX(1), oriented),
        Face3B.ZBack(AddZ(1), oriented),
    };

    public IReadOnlyCollection<Vertex3B> Vertices() => new[]
    {
        new Vertex3B(this), new Vertex3B(AddXYZ(1,1,1)),
        new Vertex3B(AddX(1)), new Vertex3B(AddXY(1,1)),
        new Vertex3B(AddY(1)), new Vertex3B(AddXZ(1,1)),
        new Vertex3B(AddZ(1)), new Vertex3B(AddYZ(1,1)),
    };

    public IReadOnlyCollection<Edge3B> Edges() => new[]
    {
        Edge3B.X(this), Edge3B.X(AddY(1)), Edge3B.X(AddYZ(1, 1)), Edge3B.X(AddZ(1)),
        Edge3B.Y(this), Edge3B.Y(AddZ(1)), Edge3B.Y(AddXZ(1, 1)), Edge3B.Y(AddX(1)),
        Edge3B.Z(this), Edge3B.Z(AddX(1)), Edge3B.Z(AddXY(1, 1)), Edge3B.Z(AddY(1)),
    };

    public IReadOnlyCollection<Cubit3B> FaceCubes() => new[]
    {
        AddX(-1), AddX(1),
        AddY(-1), AddY(1),
        AddZ(-1), AddZ(1),
    };

    public static implicit operator Cubit3B((byte X, byte Y, byte Z) tuple)
        => new(tuple.X, tuple.Y, tuple.Z);

    public static Cubit3B MinValue { get; } = (0, 0, 0);
    public static Cubit3B MaxValue { get; } = (255, 255, 255);

    public static Cubit3B Min(Cubit3B lhs, Cubit3B rhs)
        => new(
            Math.Min(lhs.X, rhs.X),
            Math.Min(lhs.Y, rhs.Y),
            Math.Min(lhs.Z, rhs.Z)
        );

    public static Cubit3B Max(Cubit3B lhs, Cubit3B rhs)
        => new(
            Math.Max(lhs.X, rhs.X),
            Math.Max(lhs.Y, rhs.Y),
            Math.Max(lhs.Z, rhs.Z)
        );
}
