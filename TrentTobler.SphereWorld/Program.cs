using System;

namespace TrentTobler.SphereWorld;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        using var window = MainWindow.CreateDefaultWindow();
        window.Run();
    }
}
