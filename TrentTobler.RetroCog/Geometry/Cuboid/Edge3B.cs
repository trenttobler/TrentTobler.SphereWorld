namespace TrentTobler.RetroCog.Geometry.Cuboid;

[Flags]
public enum Edge3BTag : byte
{
    YAxis = 0x00,
    XAxis = 0x01,
    ZAxis = 0x02,
    Reversed = 0x80,

    XNeg = XAxis | Reversed,
    YNeg = YAxis | Reversed,
    ZNeg = ZAxis | Reversed,
}

public record struct Edge3B(Cubit3B Primary, Edge3BTag Tag)
{
    public bool IsReversed => 0 != (Tag & Edge3BTag.Reversed);
    public Edge3B Reversed => new (Primary, Tag ^ Edge3BTag.Reversed);
    public Edge3B NonOriented => new(Primary, Tag & ~Edge3BTag.Reversed);

    public static Edge3B X(Cubit3B cubit, bool reversed = false)
        => new Edge3B(cubit, reversed ? Edge3BTag.XNeg : Edge3BTag.XAxis);

    public static Edge3B Y(Cubit3B cubit, bool reversed = false)
        => new Edge3B(cubit, reversed ? Edge3BTag.YNeg : Edge3BTag.YAxis);

    public static Edge3B Z(Cubit3B cubit, bool reversed = false)
        => new Edge3B(cubit, reversed ? Edge3BTag.ZNeg : Edge3BTag.ZAxis);

    public IReadOnlyCollection<Cubit3B> Cubes()
    {
        var result = (Tag & ~Edge3BTag.Reversed) switch
        {
            Edge3BTag.XAxis => new[]
            {
                Primary,
                Primary.AddY(-1),
                Primary.AddZ(-1),
                Primary.AddYZ(-1, -1),
            },

            Edge3BTag.YAxis => new[]
            {
                Primary,
                Primary.AddX(-1),
                Primary.AddZ(-1),
                Primary.AddXZ(-1, -1),
            },

            Edge3BTag.ZAxis => new[]
            {
                Primary,
                Primary.AddX(-1),
                Primary.AddY(-1),
                Primary.AddXY(-1, -1),
            },

            _ => Array.Empty<Cubit3B>(),
        };

        if (IsReversed)
            Array.Reverse(result);

        return result;
    }

    public IReadOnlyCollection<Face3B> Faces()
    {
        var result = (Tag & ~Edge3BTag.Reversed) switch
        {
            Edge3BTag.XAxis => new[]
            {
                Face3B.YBottom(Primary, IsReversed),
                Face3B.ZBack(Primary, IsReversed),
                Face3B.YBottom(Primary.AddZ(-1), IsReversed),
                Face3B.ZBack(Primary.AddY(-1), IsReversed),
            },
            Edge3BTag.YAxis => new[]
            {
                Face3B.XLeft(Primary, IsReversed),
                Face3B.ZBack(Primary, IsReversed),
                Face3B.XLeft(Primary.AddZ(-1), IsReversed),
                Face3B.ZBack(Primary.AddX(-1), IsReversed),
            },
            Edge3BTag.ZAxis => new[]
            {
                Face3B.XLeft(Primary, IsReversed),
                Face3B.YBottom(Primary, IsReversed),
                Face3B.XLeft(Primary.AddY(-1), IsReversed),
                Face3B.YBottom(Primary.AddX(-1), IsReversed),
            },
            _ => Array.Empty<Face3B>(),
        };

        if (IsReversed)
            Array.Reverse(result);
        return result;
    }

    public IReadOnlyCollection<Vertex3B> Vertices()
        => Tag switch
        {
            Edge3BTag.XAxis => new Vertex3B[] { Primary.ToXYZ(), Primary.AddX(1).ToXYZ() },
            Edge3BTag.YAxis => new Vertex3B[] { Primary.ToXYZ(), Primary.AddY(1).ToXYZ() },
            Edge3BTag.ZAxis => new Vertex3B[] { Primary.ToXYZ(), Primary.AddZ(1).ToXYZ() },

            Edge3BTag.XNeg => new Vertex3B[] { Primary.AddX(1).ToXYZ(), Primary.ToXYZ() },
            Edge3BTag.YNeg => new Vertex3B[] { Primary.AddY(1).ToXYZ(), Primary.ToXYZ() },
            Edge3BTag.ZNeg => new Vertex3B[] { Primary.AddZ(1).ToXYZ(), Primary.ToXYZ() },

            _ => Array.Empty<Vertex3B>(),
        };

    public override string ToString()
        => $"{Tag} {Primary}";
}
