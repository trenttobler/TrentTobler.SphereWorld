namespace TrentTobler.RetroCog.Geometry.Cuboid;

[Flags]
public enum Face3BTag : byte
{
    YBottomSide = 0x00,
    XLeftSide = 0x01,
    ZBackSide = 0x02,

    Reversed = 0x80,

    YTopSide = YBottomSide | Reversed,
    XRightSide = XLeftSide | Reversed,
    ZFrontSide = ZBackSide | Reversed,
}

public record struct Face3B(Cubit3B Primary, Face3BTag Tag)
{
    public (int NX, int NY, int NZ) Normal
        => Tag switch
        {
            Face3BTag.XRightSide => (1, 0, 0),
            Face3BTag.YTopSide => (0, 1, 0),
            Face3BTag.ZFrontSide => (0, 0, 1),
            Face3BTag.XLeftSide => (-1, 0, 0),
            Face3BTag.YBottomSide => (0, -1, 0),
            _ => (0, 0, -1),
        };

    private static int ReversedFlag(bool reversed) => reversed ? (1 << 31) : 0;
    public bool IsReversed => Tag.HasFlag(Face3BTag.Reversed);
    public Face3B Reversed => new (Primary, Tag ^ Face3BTag.Reversed);
    public Face3B NonOriented => new (Primary, Tag & ~Face3BTag.Reversed);

    public static Face3B YBottom(Cubit3B cubit, bool reversed = false)
        => reversed ? YTop(cubit) : new(cubit, Face3BTag.YBottomSide);

    public static Face3B YTop(Cubit3B cubit)
        => new(cubit, Face3BTag.YTopSide);

    public static Face3B XLeft(Cubit3B cubit, bool reversed = false)
        => reversed ? XRight(cubit) : new(cubit, Face3BTag.XLeftSide);

    public static Face3B XRight(Cubit3B cubit)
        => new(cubit, Face3BTag.XRightSide);

    public static Face3B ZBack(Cubit3B cubit, bool reversed = false)
        => reversed ? ZFront(cubit) : new(cubit, Face3BTag.ZBackSide);

    public static Face3B ZFront(Cubit3B cubit)
        => new(cubit, Face3BTag.ZFrontSide);

    public IReadOnlyList<Cubit3B> Cubes()
        => Tag switch
        {
            Face3BTag.XLeftSide => new[] { Primary, Primary.AddX(-1) },
            Face3BTag.YBottomSide => new[] { Primary, Primary.AddY(-1) },
            Face3BTag.ZBackSide => new[] { Primary, Primary.AddZ(-1) },

            Face3BTag.XRightSide => new[] { Primary.AddX(-1), Primary },
            Face3BTag.YTopSide => new[] { Primary.AddY(-1), Primary },
            Face3BTag.ZFrontSide => new[] { Primary.AddZ(-1), Primary },

            _ => Array.Empty<Cubit3B>(),
        };

    public IReadOnlyList<Vertex3B> Vertices()
        => Tag switch
        {
            Face3BTag.XLeftSide => new Vertex3B[]
            {
                Primary.ToXYZ(),
                Primary.AddYZ(0, 1).ToXYZ(),
                Primary.AddYZ(1, 1).ToXYZ(),
                Primary.AddYZ(1, 0).ToXYZ(),
            },
            Face3BTag.YBottomSide => new Vertex3B[]
            {
                Primary.ToXYZ(), 
                Primary.AddXZ(1, 0).ToXYZ(),
                Primary.AddXZ(1, 1).ToXYZ(),
                Primary.AddXZ(0, 1).ToXYZ(),
            },
            Face3BTag.ZBackSide => new Vertex3B[]
            {
                Primary.ToXYZ(),
                Primary.AddXY(0, 1).ToXYZ(),
                Primary.AddXY(1, 1).ToXYZ(),
                Primary.AddXY(1, 0).ToXYZ(),
            },

            Face3BTag.XRightSide => new Vertex3B[]
            {
                Primary.AddYZ(1, 0).ToXYZ(),
                Primary.AddYZ(1, 1).ToXYZ(),
                Primary.AddYZ(0, 1).ToXYZ(),
                Primary.ToXYZ(),
            },
            Face3BTag.YTopSide => new Vertex3B[]
            {
                Primary.AddXZ(0, 1).ToXYZ(),
                Primary.AddXZ(1, 1).ToXYZ(),
                Primary.AddXZ(1, 0).ToXYZ(),
                Primary.ToXYZ(),
            },
            Face3BTag.ZFrontSide => new Vertex3B[]
            {
                Primary.AddXY(1, 0).ToXYZ(),
                Primary.AddXY(1, 1).ToXYZ(),
                Primary.AddXY(0, 1).ToXYZ(),
                Primary.ToXYZ(),
            },

            _ => Array.Empty<Vertex3B>(),
        };

    public IReadOnlyCollection<Edge3B> OrientedEdges()
        => Edges(true);

    public IReadOnlyCollection<Edge3B> NonOrientedEdges()
        => Edges(false);

    private IReadOnlyCollection<Edge3B> Edges(bool oriented = false)
    {
        var reversed = oriented && IsReversed;

        var result = (Tag & ~Face3BTag.Reversed) switch
        {
            Face3BTag.YBottomSide => new[]
            {
                Edge3B.X(Primary, reversed),
                Edge3B.Z(Primary, reversed),
                Edge3B.X(Primary.AddZ(1), reversed),
                Edge3B.Z(Primary.AddX(1), reversed),
            },
            Face3BTag.XLeftSide => new[]
            {
                Edge3B.Y(Primary, reversed),
                Edge3B.Z(Primary, reversed),
                Edge3B.Y(Primary.AddZ(1), reversed),
                Edge3B.Z(Primary.AddY(1), reversed),
            },
            _ => new[]
            {
                Edge3B.X(Primary, reversed),
                Edge3B.Y(Primary, reversed),
                Edge3B.X(Primary.AddY(1), reversed),
                Edge3B.Y(Primary.AddX(1), reversed),
            }
        };
        if (reversed)
            Array.Reverse(result);
        return result;
    }

    public override string ToString()
        => $"{Tag} {Primary}";
}
