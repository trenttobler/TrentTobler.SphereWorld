using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace TrentTobler.RetroCog.Graphics;

public class GlApi : IGlApi
{
    // private ILogger<GlApi> Logger { get; }

    private void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        System.Diagnostics.Debugger.Break();
    }

    public GlApi(ILogger<GlApi> logger)
    {
        // Logger = logger;
        GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
    }

    public ErrorCode GetError() => GL.GetError();

    public int CreateProgram() => GL.CreateProgram();
    public int CreateShader(ShaderType shaderType) => GL.CreateShader(shaderType);
    public void ShaderSource(int shader, string code) => GL.ShaderSource(shader, code);
    public void CompileShader(int shader) => GL.CompileShader(shader);
    public void GetShader(int shader, ShaderParameter shaderParameter, out int result) => GL.GetShader(shader, shaderParameter, out result);
    public string GetShaderInfoLog(int shader) => GL.GetShaderInfoLog(shader);
    public void DeleteShader(int shader) => GL.DeleteShader(shader);
    public void AttachShader(int program, int shader) => GL.AttachShader(program, shader);
    public void LinkProgram(int program) => GL.LinkProgram(program);
    public void GetProgram(int program, GetProgramParameterName programParameter, out int result) => GL.GetProgram(program, programParameter, out result);
    public string GetProgramInfoLog(int program) => GL.GetProgramInfoLog(program);
    public void DetachShader(int program, int shader) => GL.DetachShader(program, shader);
    public void DeleteProgram(int program) => GL.DeleteProgram(program);
    public void UseProgram(int program) => GL.UseProgram(program);

    public int GetUniformLocation(int program, string name) => GL.GetUniformLocation(program, name);
    public void Uniform1(int uniform, int value) => GL.Uniform1(uniform, value);
    public void Uniform1(int uniform, float value) => GL.Uniform1(uniform, value);
    public void Uniform2(int uniform, Vector2 value) => GL.Uniform2(uniform, ref value);
    public void UniformMatrix4(int uniform, bool transpose, ref Matrix4 matrix) => GL.UniformMatrix4(uniform, transpose, ref matrix);
    public void DrawElements(PrimitiveType mode, int count, DrawElementsType drawType, int indices) => GL.DrawElements(mode, count, drawType, indices);

    public int GenVertexArray() => GL.GenVertexArray();
    public int GenBuffer() => GL.GenBuffer();
    public void BindVertexArray(int vao) => GL.BindVertexArray(vao);
    public void BindBuffer(BufferTarget target, int buffer) => GL.BindBuffer(target, buffer);
    public void BufferData<T>(BufferTarget target, int size, T[] data, BufferUsageHint hint)
        where T : struct
        => GL.BufferData(target, size, data, hint);
    public void BufferData<T>(BufferTarget target, Span<T> span, BufferUsageHint hint)
        where T : struct
    {
        if (span.Length == 0)
            return;
        GL.BufferData(target, Marshal.SizeOf<T>() * span.Length, ref span[0], hint);
    }
    public void DeleteBuffer(int buffer) => GL.DeleteBuffer(buffer);
    public void DeleteVertexArray(int vertexArray) => GL.DeleteVertexArray(vertexArray);

    public int GetAttribLocation(int program, string name) => GL.GetAttribLocation(program, name);
    public void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, IntPtr offset)
        => GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
    public void EnableVertexArrayAttrib(int index) => GL.EnableVertexAttribArray(index);

    public int GenTexture() => GL.GenTexture();
    public void ActiveTexture(TextureUnit unit) => GL.ActiveTexture(unit);
    public void BindTexture(TextureTarget target, int texture) => GL.BindTexture(target, texture);
    public void TexParameter(TextureTarget target, TextureParameterName parameter, int value)
        => GL.TexParameter(target, parameter, value);

    public void TexImage2D<T>(
        TextureTarget target,
        int level,
        PixelInternalFormat pixelInternalFormat,
        int width, int height,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T : struct
        => GL.TexImage2D(
            target,
            level,
            pixelInternalFormat,
            width, height, 0,
            pixelFormat,
            pixelType,
            ref pixelData[0]);

    public void TexImage1D<T>(
        TextureTarget target,
        int level,
        PixelInternalFormat pixelInternalFormat,
        int width,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T : struct
        => GL.TexImage1D(
            target,
            level,
            pixelInternalFormat,
            width, 0,
            pixelFormat,
            pixelType,
            ref pixelData[0]);

    public void TexSubImage2D<T>(
        TextureTarget target,
        int level,
        int xoffset, int yoffset, int width, int height,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T : struct
        => GL.TexSubImage2D(
            target,
            level,
            xoffset, yoffset, width, height,
            pixelFormat,
            pixelType,
            ref pixelData[0]);

    public void ClearColor(float r, float g, float b, float a) => GL.ClearColor(r, g, b, a);
    public void Viewport(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void Clear(ClearBufferMask mask) => GL.Clear(mask);
    public void Enable(EnableCap caps) => GL.Enable(caps);
    public void Disable(EnableCap caps) => GL.Disable(caps);
    public void DepthFunc(DepthFunction fn) => GL.DepthFunc(fn);
    public void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor) => GL.BlendFunc(sfactor, dfactor);
}
