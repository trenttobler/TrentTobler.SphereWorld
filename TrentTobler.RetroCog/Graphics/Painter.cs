using OpenTK.Graphics.OpenGL;
using System.Linq.Expressions;
using TrentTobler.RetroCog.Collections;

namespace TrentTobler.RetroCog.Graphics;

public interface IVertexAttribBuilder<TVertex> where TVertex : struct
{
    IVertexAttribBuilder<TVertex> Attrib<TAttrib>(
        string name,
        Expression<Func<TVertex, TAttrib>> getter)
        where TAttrib : struct;
}

public class VertexAttribBuilder<TVertex> : IVertexAttribBuilder<TVertex>
    where TVertex : struct
{
    private IGlApi GlApi { get; }
    private int Program { get; }

    public VertexAttribBuilder(IGlApi glapi, int program)
    {
        GlApi = glapi;
        Program = program;
    }

    public IVertexAttribBuilder<TVertex> Attrib<TAttrib>(
        string name,
        Expression<Func<TVertex, TAttrib>> getter)
        where TAttrib : struct
    {
        var index = GlApi.GetAttribLocation(Program, name);
        GlApi.VertexAttribPointer(index, getter);
        GlApi.EnableVertexArrayAttrib(index);
        return this;
    }
}

public class Painter : IDisposable
{
    public int Program { get; }

    public int Vao { get; }
    public int Vbo { get; }
    public int Ebo { get; }

    protected IGlApi GlApi { get; }

    private int _disposed;

    public Painter(IGlApi glapi, int program)
    {
        GlApi = glapi;
        Program = program;

        Vao = GlApi.GenVertexArray();
        Vbo = GlApi.GenBuffer();
        Ebo = GlApi.GenBuffer();
    }

    public IVertexAttribBuilder<TVertex> BindMesh<TVertex>(
        Span<TVertex> vertices,
        VertexIndexList elements)
        where TVertex: struct
    {
        GlApi.BindVertexArray(Vao);

        GlApi.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GlApi.BufferData(BufferTarget.ArrayBuffer, vertices, BufferUsageHint.DynamicDraw);

        GlApi.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
        GlApi.BufferData(BufferTarget.ElementArrayBuffer, elements, BufferUsageHint.DynamicDraw);

        return new VertexAttribBuilder<TVertex>(GlApi, Program);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        DeleteResources();

        GC.SuppressFinalize(this);
    }

    protected virtual void DeleteResources()
    {
        GlApi.DeleteBuffer(Vbo);
        GlApi.DeleteBuffer(Ebo);
        GlApi.DeleteVertexArray(Vao);
    }
}
