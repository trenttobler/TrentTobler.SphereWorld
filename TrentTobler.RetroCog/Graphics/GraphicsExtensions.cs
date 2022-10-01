using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using TrentTobler.RetroCog.Collections;

namespace TrentTobler.RetroCog.Graphics;

public static class GraphicsExtensions
{
    public static void BufferData(this IGlApi glApi, BufferTarget target, VertexIndexList verts, BufferUsageHint hint)
    {
        if (verts.TryByteSpan(out var byteSpan))
            glApi.BufferData(target, byteSpan, hint);

        else if (verts.TryShortSpan(out var shortSpan))
            glApi.BufferData(target, shortSpan, hint);

        else if (verts.TryIntSpan(out var intSpan))
            glApi.BufferData(target, intSpan, hint);

        else
            throw new InvalidOperationException($"{verts.ElementType}: invalid element type");
    }

    public static void VertexAttribPointer<TItem, TField>(this IGlApi glApi, int index, Expression<Func<TItem, TField>> expr, bool normalized = false)
        where TItem : struct
        where TField : struct
    {
        var (fieldType, offset) = GetFieldLayout(expr);
        var stride = Marshal.SizeOf<TItem>();
        var attrib = ShaderVertexAttribTable[fieldType];
        glApi.VertexAttribPointer(index, attrib.Size, attrib.PointerType, normalized, stride, offset);
    }

    private record AttribTypeDef(
        Type CSharpType,
        int Size,
        VertexAttribPointerType PointerType,
        string GlslTypeName
    );

    private static Dictionary<Type, AttribTypeDef> ShaderVertexAttribTable = new[]
    {
        new AttribTypeDef( typeof(float), 1, VertexAttribPointerType.Float, "float"),
        new AttribTypeDef( typeof(Vector2), 2, VertexAttribPointerType.Float, "vec2"),
        new AttribTypeDef( typeof(Vector3), 3, VertexAttribPointerType.Float, "vec3"),
        new AttribTypeDef( typeof(Vector4), 4, VertexAttribPointerType.Float, "vec4"),
    }.ToDictionary(x => x.CSharpType);

    private static (Type type, IntPtr offset) GetFieldLayout<TItem, TField>(Expression<Func<TItem, TField>> expr)
    {
        switch (expr.Body)
        {
            case MemberExpression member:
                if (member.Member is FieldInfo field)
                    return (field.FieldType, Marshal.OffsetOf<TItem>(field.Name));
                break;

            case ParameterExpression parameter:
                if (parameter == expr.Parameters.FirstOrDefault())
                    return (parameter.Type, IntPtr.Zero);
                break;
        }

        throw new NotImplementedException($"TODO: implement ability to extract field name from expression: {expr}");
    }
}
