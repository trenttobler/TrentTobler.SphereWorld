using System.Diagnostics;
using System.IO;
using System.Text;

namespace TrentTobler.SphereWorld;

public class DebugWriter : TextWriter
{
    public override Encoding Encoding => Encoding.ASCII;
    public override void Write(char value)
        => Debug.Write(value);
    public static DebugWriter Instance { get; } = new DebugWriter();
}
