using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using TrentTobler.RetroCog;
using Microsoft.Extensions.Logging;
using TrentTobler.RetroCog.Graphics;
using TrentTobler.RetroCog.Collections;
using TrentTobler.RetroCog.Geometry;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;

namespace TrentTobler.SphereWorld;

public class MainWindow : GameWindow
{
    // For debugging and benchmarking for now...
    private const int CuboidLimit = 800;
    private const float MovementSpeed = 1.00f;
    private const float DistanceAboveGround = 0.1f;

    // TODO: these should be injected
    private static ILoggerFactory Logs { get; } = LoggerFactory.Create(builder => builder.AddDebug());
    public static ILogger<T> GetLogger<T>() => Logs.CreateLogger<T>();

    private IGlApi? _glApi = null;
    private IGlApi GlApi => _glApi ??= new GlApi(GetLogger<GlApi>());
      
    private IShaderFactory? _shaders = null;

    private IAssetProvider AssetProvider { get; } = FileAssetProvider.Instance;
    private IShaderFactory ShaderFactory => _shaders ??= new ShaderFactory(FileAssetProvider.Instance, GlApi, GetLogger<ShaderFactory>());

    private readonly Stopwatch _stopwatch = new Stopwatch();
    private Matrix4 _model = Matrix4.Identity;

    private readonly Camera _camera = new()
    {
        Eye = new Vector3(00f, -300f, -0f),
    };

    private Matrix4 _projection = Matrix4.Identity;
    private Matrix4 _screenView = Matrix4.Identity;

    public static MainWindow CreateDefaultWindow()
    {
        var nativeWindowSettings = new NativeWindowSettings
        {
            Size = new Vector2i(2400, 1800),
            Title = "Cuboid World",
            Flags = ContextFlags.ForwardCompatible,
        };
        return new MainWindow(nativeWindowSettings);
    }

    private static readonly Random _random = new(101);

    public MainWindow(NativeWindowSettings nativeWindowSettings)
    : base(
        new GameWindowSettings
        {
            RenderFrequency = 0.0,
            UpdateFrequency = 0.0,
        },
        nativeWindowSettings)
    {
        WindowedSize = nativeWindowSettings.Size;
    }

    ScreenMemory ScreenConsole { get; } = new ScreenMemory(50, 30);

    private ScreenPainter? _screenPainter;
    ScreenPainter ScreenPainter => _screenPainter ??= new ScreenPainter(
        GlApi,
        AssetProvider,
        ShaderFactory,
        ScreenConsole);

    public bool ShowScreen { get; set; }

    protected override void OnLoad()
    {
        base.OnLoad();

        CursorState |= CursorState.Grabbed;

        GlApi.Enable(EnableCap.Texture2D);
        GlApi.ClearColor(0, 0, 0, 1);

#if TEST_SCREEN_CONSOLE_COLORS
        for (int bg = 0; bg < 16; ++bg)
        {
            for (int fg = 0; fg < 16; ++fg)
            {
                ScreenConsole.SetColors(fg, bg);
                ScreenConsole.Write($"{bg + fg * 16:X2}");
            }
            ScreenConsole.WriteLine();
        }
#endif

        var program = ShaderFactory.Compile("Shaders/SimpleColor");
        LoadTexture();

        World = CuboidWorld.GenerateWorld(CuboidLimit);

        // Choose a random cubit.
        // Then move up to find a non-solid cubit.  Assumes there is one
        // (need to add a guard to generation to clear out a field on the border of the cubitrix
        //  to guarantee this.)
        var cubit = World.Dirt.SelectAtRandom(_random);
        var face = World.Cubitrix.Faces(cubit)
            .SelectAtRandom(_random);

        var center = face.Vertices().Select(vert => new Vector3(vert.X, vert.Y, vert.Z)).Average();
        var norm = new Vector3(face.Normal.NX, face.Normal.NY, face.Normal.NZ);

        _camera.Eye = center + norm * 0.1f;
        _camera.Up = norm;
        _camera.Heading = Vector3.Cross(new Vector3(1f, 1f, 1f), norm).Normalized();

        Painter = new CuboidWorldPainter(GlApi, program);

        Painter.ApplyMesh(World.Mesh, PrimitiveType.Triangles);

        UpdateWorld(0, 0);
        UpdateViewport();

        _stopwatch.Start();
    }

    private int MeshTexture { get; set; }

    private void LoadTexture()
    {
        using var stream = AssetProvider.OpenRead("Images", "leaf.png");
        var image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
        var frame = image.Frames[0];
        var (width, height) = (frame.Width, frame.Height);

        var pixels = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        MeshTexture = GlApi.GenTexture();
        GlApi.ActiveTexture(TextureUnit.Texture4);
        GlApi.BindTexture(TextureTarget.Texture2D, MeshTexture);
        GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GlApi.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            width, height,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            pixels.AsSpan());
    }

    CuboidWorld World { get; set; }
    CuboidWorldPainter Painter { get; set; }

    private void UpdateWorld(double dt, double totalT)
    {
    }

    private void UpdateViewport()
    {
        var width = Size.X;
        var height = Size.Y;

        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(70f),
            (float)width / height,
            .001f, 100f);
        GlApi.Viewport(0, 0, width, height);

        // Position the screen on the left side.
        _screenView = width < height
            ? Matrix4.CreateOrthographicOffCenter(
                -1f, 0f,
                1f - 2 * (float)width / height, 1f,
                -1, 1).Inverted()
            : Matrix4.CreateOrthographicOffCenter(
                -1f, -1f + (float)height / width,
                -1f, 1f,
                -1, 1).Inverted();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        UpdateViewport();
    }

    const float FocusAnglePerClick = 0.025f * (float)(Math.PI / 180);
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        _camera.Focus = Math.Min(Math.Max(_camera.Focus + e.DeltaY * FocusAnglePerClick, (float)Math.PI / -2), (float)Math.PI / 2);
        _camera.Heading = Matrix3.CreateFromAxisAngle(_camera.Up, e.DeltaX * FocusAnglePerClick)
            * _camera.Heading;
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        var startTime = _stopwatch.Elapsed;

        switch (e.Key)
        {
            case Keys.Escape:
                Close();
                break;

            case Keys.Space:
                ShowScreen = !ShowScreen;
                ShowFps = ShowScreen;
                break;

            case Keys.Tab:
                Painter.ApplyMesh(
                    World.Mesh,
                    Painter.Mode == PrimitiveType.Lines ? PrimitiveType.Triangles
                        : PrimitiveType.Lines);
                break;

            case Keys.D1:
                World.LevelOfDetail = 0;
                Painter.ApplyMesh(World.Mesh, Painter.Mode);
                break;

            case Keys.D2:
                World.LevelOfDetail = 1;
                Painter.ApplyMesh(World.Mesh, Painter.Mode);
                break;

            case Keys.D3:
                World.LevelOfDetail = 2;
                Painter.ApplyMesh(World.Mesh, Painter.Mode);
                break;

            case Keys.D4:
                World.LevelOfDetail = 3;
                Painter.ApplyMesh(World.Mesh, Painter.Mode);
                break;

            case Keys.D5:
                World.LevelOfDetail = 4;
                Painter.ApplyMesh(World.Mesh, Painter.Mode);
                break;

            case Keys.F11:
                if (0 == (WindowState & WindowState.Fullscreen))
                {
                    WindowedSize = Size;
                    WindowState |= WindowState.Fullscreen;
                    ScreenConsole.SetColors(ScreenPainter.White, ScreenPainter.Black);
                    ScreenConsole.WriteLine($"full screen {Size}");
                }
                else
                {
                    WindowState &= ~WindowState.Fullscreen;
                    Size = WindowedSize;
                    ScreenConsole.SetColors(ScreenPainter.White, ScreenPainter.Black);
                    ScreenConsole.WriteLine($"back to windowed {Size}");
                }
                UpdateViewport();
                break;
        }

        var endTime = _stopwatch.Elapsed;
        var totalTime = endTime - startTime;
        if (totalTime > TimeSpan.FromSeconds(0.02)) // shoot for 50 fps minimum
        {
            ScreenConsole.SetColors(ScreenPainter.FlamingoPink, ScreenPainter.Lemon);
            ScreenConsole.WriteLine($"slow: {totalTime} sec");
        }

        base.OnKeyDown(e);
    }

    private Vector2i WindowedSize { get; set; }

    T KeyCheck<T>(T up, T down, params Keys[] keys)
        => keys.Any(key => KeyboardState.IsKeyDown(key)) ? down : up;

    Vector2 GetMotion()
        => new(
            KeyCheck(0, -1, Keys.Left, Keys.A) + KeyCheck(0, 1, Keys.Right, Keys.D),
            KeyCheck(0, 1, Keys.Up, Keys.W) + KeyCheck(0, -1, Keys.Down, Keys.S));

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        MovePlayer(args.Time);

        var tick = _stopwatch.Elapsed;
        var t = tick.TotalSeconds;
        UpdateWorld(args.Time, t);

        ScreenPainter.Update();
    }

    private void MovePlayer(double time)
    {
        var motion = GetMotion() * MovementSpeed * (float)time;
        _camera.Forward(motion.Y);
        _camera.Strafe(motion.X);

        var (herePos, hereNorm) = World.GetBestSurface(_camera.Eye, _camera.Up);
        var nextPos = herePos + hereNorm * DistanceAboveGround;
        var (_, up) = World.GetBestSurface(nextPos + _camera.Heading * 0.1f, _camera.Up);
        var heading = Vector3.Cross(up, _camera.Right);
        up.Normalize();
        heading.Normalize();

        _camera.Eye = nextPos;
        _camera.Up = hereNorm;
        _camera.Heading = heading;
    }

    DateTime _lastTime = DateTime.UtcNow;
    int _frameCount = 0;
    bool ShowFps { get; set; } = false;

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GlApi.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GlApi.Enable(EnableCap.DepthTest);
        GlApi.DepthFunc(DepthFunction.Less);

        TrackFps();

        Painter.Draw(_camera.View, _projection, _model, MeshTexture);

        if (ShowScreen)
        {
            GlApi.Disable(EnableCap.DepthTest);
            GlApi.Enable(EnableCap.Blend);
            GlApi.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            ScreenPainter.Alpha = 0.5f;
            ScreenPainter.Draw(_screenView);
        }

        SwapBuffers();
    }

    private void TrackFps()
    {
        ++_frameCount;
        var now = DateTime.UtcNow;
        if (now - _lastTime > TimeSpan.FromSeconds(10) || ShowFps)
        {
            ShowFps = false;
            var elementsPerFrame = Painter.Mode == PrimitiveType.Triangles
                ? Painter.Count / 3.0
                : Painter.Count / 2.0;

            var framesPerSec = _frameCount / (now - _lastTime).TotalSeconds;

            ScreenConsole.SetColors(ScreenPainter.MintGreen, ScreenPainter.Black);
            ScreenConsole.WriteLine($"{framesPerSec:N0} fps, {elementsPerFrame.ToMetric()} {Painter.Mode}/frame = {(elementsPerFrame * framesPerSec).ToMetric()}/sec");
            ScreenConsole.WriteLine($"    Model: {World.Mesh.Faces.Count} faces, {World.Mesh.Vertices.Count} vertices");

            _lastTime = now;
            _frameCount = 0;
        }
    }
}
