using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace TrentTobler.RetroCog.Geometry;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct Vertex : IEquatable<Vertex>
{
    public readonly Vector3 Position;
    public readonly Vector2 TexCoord;
    public readonly Vector3 Normal;

    public Vertex(Vector3 pos, Vector2 tex, Vector3 norm)
    {
        Position = pos;
        TexCoord = tex;
        Normal = norm;
    }

    public override string ToString()
        => $"({Position.X}, {Position.Y}, {Position.Z})[{TexCoord.X} {TexCoord.Y}] {Normal}";

    public static Vertex Zero { get; } = new Vertex(Vector3.Zero, Vector2.Zero, Vector3.Zero);

    public static Vertex operator +(Vertex lhs, Vertex rhs)
        => new (
            lhs.Position + rhs.Position,
            lhs.TexCoord + rhs.TexCoord,
            lhs.Normal + rhs.Normal
        );

    public static Vertex operator -(Vertex lhs, Vertex rhs)
        => new(
            lhs.Position - rhs.Position,
            lhs.TexCoord - rhs.TexCoord,
            lhs.Normal - rhs.Normal
        );

    public static Vertex operator -(Vertex arg)
        => new (
            -arg.Position,
            -arg.TexCoord,
            -arg.Normal
        );

    public static Vertex operator *(float lhs, Vertex rhs)
        => rhs * lhs;

    public static Vertex operator *(Vertex lhs, float rhs)
        => new(
            lhs.Position * rhs,
            lhs.TexCoord * rhs,
            lhs.Normal * rhs
        );

    public static Vertex operator /(Vertex lhs, float rhs)
        => new(
            lhs.Position / rhs,
            lhs.TexCoord / rhs,
            lhs.Normal / rhs
        );
}

public static class VertexExtensions
{
    public static (Vertex, int) SumCount(this IEnumerable<Vertex> vertices)
    {
        using var v = vertices.GetEnumerator();
        if (!v.MoveNext())
            return (Vertex.Zero, 0);

        var sum = v.Current;
        if (!v.MoveNext())
            return (sum, 1);

        var cnt = 2;
        sum += v.Current;
        while (v.MoveNext())
        {
            sum += v.Current;
            cnt++;
        }

        return (sum, cnt);
    }

    public static Vertex Average(this IEnumerable<Vertex> vertices)
        => vertices.SumCount() switch
        {
            (Vertex zero, 0) => zero,
            (Vertex one, 1) => one,
            (Vertex sum, int cnt) => sum / cnt,
        };

    public static Vertex Midpoint(this Vertex lhs, Vertex rhs)
        => (lhs + rhs) / 2;
}
