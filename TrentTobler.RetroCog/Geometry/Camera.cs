using OpenTK.Mathematics;

namespace TrentTobler.RetroCog.Geometry;

public class Camera
{
    public Vector3 Eye { get; set; } = new Vector3(0, -1, 0);
    public Vector3 Heading { get; set; } = new Vector3(0, 1, 0);
    public Vector3 Up { get; set; } = new Vector3(0, 0, 1);
    public Vector3 Right => Vector3.Cross(Heading, Up);

    public Matrix4 View
    {
        get
        {
            var focus = Matrix3.CreateFromAxisAngle(Right, Focus);
            return Matrix4.LookAt(
                Eye,
                Eye + focus * Heading,
                focus * Up
            );
        }
    }

    public float Focus { get; set; } = 0;

    public void Forward(float distance)
    {
        Eye = Eye + Heading * distance;
    }

    public void Elevate(float distance)
    {
        Eye = Eye + Up * distance;
    }

    public void Strafe(float distance)
    {
        Eye = Eye + Right * distance;
    }

    public void Incline(float angleUp)
    {
        var right = Right;
        var rot = Matrix4.CreateFromAxisAngle(right, MathHelper.DegreesToRadians(angleUp));
        var heading = rot * new Vector4(Heading, 1);

        Heading = heading.Xyz.Normalized();
        Up = Vector3.Cross(Heading, right);
    }

    public void Turn(float angleLeft)
    {
        var rot = Matrix4.CreateFromAxisAngle(Up, MathHelper.DegreesToRadians(angleLeft));
        var heading = rot * new Vector4(Heading, 1);
        Heading = heading.Xyz.Normalized();
    }

    public void Spin(float angleClock)
    {
        var rot = Matrix4.CreateFromAxisAngle(Heading, MathHelper.DegreesToRadians(-angleClock));
        var up = rot * new Vector4(Up, 1);
        Up = up.Xyz.Normalized();
    }

    public override string ToString()
        => $"Eye={Eye} Heading={Heading} Up={Up}";
}
