using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TrentTobler.RetroCog.Graphics;

public interface IGlApi
{
    ErrorCode GetError();

    int CreateProgram();
    int CreateShader(ShaderType shaderType);
    void ShaderSource(int shader, string code);
    void CompileShader(int shader);
    void GetShader(int shader, ShaderParameter shaderParameter, out int result);
    string GetShaderInfoLog(int shader);
    void DeleteShader(int shader);
    void AttachShader(int program, int shader);
    void LinkProgram(int program);
    void GetProgram(int program, GetProgramParameterName programParameter, out int result);
    string GetProgramInfoLog(int program);
    void DetachShader(int program, int shader);
    void DeleteProgram(int program);
    void UseProgram(int program);

    int GetUniformLocation(int program, string name);
    void Uniform1(int uniform, float value);
    void Uniform1(int uniform, int value);
    void Uniform2(int uniform, Vector2 value);
    void UniformMatrix4(int uniform, bool transpose, ref Matrix4 matrix);
    void DrawElements(PrimitiveType mode, int count, DrawElementsType drawType, int indices);

    int GenVertexArray();
    int GenBuffer();
    void BindVertexArray(int vao);
    void BindBuffer(BufferTarget target, int buffer);
    void BufferData<T>(BufferTarget target, int size, T[] data, BufferUsageHint hint) where T : struct;
    void BufferData<T>(BufferTarget target, Span<T> data, BufferUsageHint hint) where T : struct;
    void DeleteBuffer(int buffer);
    void DeleteVertexArray(int vertexArray);

    int GetAttribLocation(int program, string name);
    void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, IntPtr offset);
    void EnableVertexArrayAttrib(int index);

    int GenTexture();
    void ActiveTexture(TextureUnit unit);
    void BindTexture(TextureTarget target, int texture);
    void TexParameter(TextureTarget target, TextureParameterName parameter, int value);
    void TexImage2D<T>(
        TextureTarget target,
        int level,
        PixelInternalFormat pixelInternalFormat,
        int width, int height,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T: struct;
    void TexImage1D<T>(
        TextureTarget target,
        int level,
        PixelInternalFormat pixelInternalFormat,
        int width,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T : struct;
    void TexSubImage2D<T>(
        TextureTarget target,
        int level,
        int xoffset, int yOffset, int width, int height,
        PixelFormat pixelFormat,
        PixelType pixelType,
        Span<T> pixelData) where T : struct;

    void ClearColor(float r, float g, float b, float a);
    void Viewport(int x, int y, int width, int height);

    void Clear(ClearBufferMask mask);
    void Enable(EnableCap caps);
    void Disable(EnableCap caps);
    void DepthFunc(DepthFunction fn);
    void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor);
}
