using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using TrentTobler.RetroCog.Geometry;
using TrentTobler.RetroCog.Graphics;

namespace TrentTobler.SphereWorld;

public class CuboidWorldPainter : Painter
{
    private int ViewUniform { get; }
    private int ProjectionUniform { get; }
    private int ModelUniform { get; }
    private int TexImageUniform { get; }

    public int Count { get; set; }
    private DrawElementsType ElementType { get; set; }

    public PrimitiveType Mode { get; set; }

    public void ApplyMesh(Mesh<Vertex> mesh, PrimitiveType mode)
    {
        Mode = mode;

        var (vertices, faces) = mode switch
        {
            PrimitiveType.Lines => mesh.ToOutlineElements(),
            PrimitiveType.Triangles => mesh.ToTriangulatedElements(),
            _ => throw new ArgumentException(),
        };

        BindMesh(vertices.AsSpan(), faces)
            .Attrib("aPos", v => v.Position)
            .Attrib("tPos", v => v.TexCoord)
            .Attrib("norm", v => v.Normal);

        (Count, ElementType) = (faces.Count, faces.ElementType);
    }

    public CuboidWorldPainter(
        IGlApi glApi,
        int program)
        : base(glApi, program)
    {
        ViewUniform = GlApi.GetUniformLocation(program, "view");
        ProjectionUniform = GlApi.GetUniformLocation(program, "projection");
        ModelUniform = GlApi.GetUniformLocation(program, "model");
        TexImageUniform = GlApi.GetUniformLocation(program, "texImage");
    }

    public void Draw(Matrix4 view, Matrix4 projection, Matrix4 model, int texImage)
    {
        GlApi.UseProgram(Program);

        GlApi.ActiveTexture(TextureUnit.Texture0);
        GlApi.BindTexture(TextureTarget.Texture2D, texImage);
        GlApi.Uniform1(TexImageUniform, 0);

        GlApi.UniformMatrix4(ViewUniform, true, ref view);
        GlApi.UniformMatrix4(ProjectionUniform, true, ref projection);
        GlApi.UniformMatrix4(ModelUniform, true, ref model);
        GlApi.BindVertexArray(Vao);
        GlApi.DrawElements(Mode, Count, ElementType, 0);
    }
}
