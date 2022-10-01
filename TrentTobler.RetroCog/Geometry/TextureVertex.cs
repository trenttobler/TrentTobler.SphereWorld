using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace TrentTobler.RetroCog.Geometry;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct TextureVertex
{
    public Vector4 Position;
    public Vector3 Texture;
    public Vector3 Normal;
}
