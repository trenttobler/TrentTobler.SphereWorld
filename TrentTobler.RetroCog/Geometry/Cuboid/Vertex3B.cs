namespace TrentTobler.RetroCog.Geometry.Cuboid;

public record struct Vertex3B(Cubit3B Pos)
{
    public byte X => Pos.X;
    public byte Y => Pos.Y;
    public byte Z => Pos.Z;

    public IReadOnlyCollection<Cubit3B> Cubes() => new[]
    {
        Pos,
        Pos.AddX(-1),
        Pos.AddY(-1),
        Pos.AddZ(-1),
        Pos.AddXY(-1,-1),
        Pos.AddXZ(-1,-1),
        Pos.AddYZ(-1,-1),
        Pos.AddXYZ(-1,-1,-1),
    };

    public IReadOnlyCollection<Face3B> Faces() => new[]
    {
        Face3B.YBottom(Pos),
        Face3B.YBottom(Pos.AddX(-1)),
        Face3B.YBottom(Pos.AddXZ(-1, -1)),
        Face3B.YBottom(Pos.AddZ(-1)),
        Face3B.XLeft(Pos),
        Face3B.XLeft(Pos.AddY(-1)),
        Face3B.XLeft(Pos.AddYZ(-1,-1)),
        Face3B.XLeft(Pos.AddZ(-1)),
        Face3B.ZBack(Pos),
        Face3B.ZBack(Pos.AddX(-1)),
        Face3B.ZBack(Pos.AddXY(-1,-1)),
        Face3B.ZBack(Pos.AddY(-1)),
    };

    public IReadOnlyCollection<Edge3B> Edges() => new[]
    {
        Edge3B.X(Pos),
        Edge3B.Y(Pos),
        Edge3B.Z(Pos),
        Edge3B.X(Pos.AddX(-1)),
        Edge3B.Y(Pos.AddY(-1)),
        Edge3B.Z(Pos.AddZ(-1)),
    };

    public override string ToString() => $"Vertex {Pos}";

    public static implicit operator Vertex3B((byte X, byte Y, byte Z) tuple)
        => new Vertex3B(tuple);
}
