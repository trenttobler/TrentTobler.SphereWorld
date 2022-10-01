using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using TrentTobler.RetroCog.Collections;

namespace TrentTobler.RetroCog.Graphics
{
    public class ScreenPainter
    {
        public interface IScreen
        {
            int Width { get; }
            int Height { get; }

            Span<Symbol> AsSpan();
            Span<Symbol> AsInvalidSpan(out int xoffset, out int yoffset, out int width, out int height);
            void ClearInvalid();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public record struct Symbol
        {
            public byte Token;
            public byte Color;
        }

        public const int Black = 0;              // 00 00 00
        public const int Grey3 = 1;              // 3F 3F 3F
        public const int Grey2 = 2;              // 7F 7F 7F
        public const int Grey1 = 3;              // BF BF BF
        public const int White = 4;              // FF FF FF
        public const int Lemon = 5;              // FF FF 7F
        public const int LavenderRose = 6;       // FF AA FF
        public const int LightSalmon = 7;        // FF AA 7F
        public const int FlamingoPink = 8;       // FF 55 FF
        public const int Watermelon = 9;         // FF 55 7F
        public const int ElectricBlue = 10;      // 7F FF FF
        public const int MintGreen = 11;         // 7F FF 7F
        public const int MayaBlue = 12;          // 7F AA FF
        public const int AmuletGreen = 13;       // 7F AA 7F
        public const int LightSlateBlue = 14;    // 7F 55 FF
        public const int TrendyPink = 15;        // 7F 55 7F

        private IGlApi GlApi { get; }

        public float Alpha { get; set; } = 1.0f;

        private int Program { get; }
        private Painter Painter { get; }

        private int AlphaUniform { get; }
        private int ScreenSizeUniform { get; }

        private int FontTexture { get; }
        private int FontUniform { get; }

        private int ScreenTexture { get; }
        private int ScreenUniform { get; }

        private int ColorsTexture { get; }
        private int ColorsUniform { get; }

        private int ViewUniform { get; }

        public IScreen Screen { get; }

        public ScreenPainter(
            IGlApi glApi,
            IAssetProvider assets,
            IShaderFactory shaderFactory,
            IScreen screen)
        {
            GlApi = glApi;

            Screen = screen;

            Program = shaderFactory.Compile("Shaders/ScreenText");
            Painter = new Painter(GlApi, Program);
            Painter.BindMesh(
                new Vector2[]
                {
                    new Vector2(-1, -1),
                    new Vector2(+1, -1),
                    new Vector2(+1, +1),
                    new Vector2(-1, +1),
                }.AsSpan(),
                new VertexIndexList
                {
                    0, 1, 2,
                    2, 3, 0
                })
                .Attrib("aPos", v => v);

            ScreenUniform = GlApi.GetUniformLocation(Program, "screen");
            FontUniform = GlApi.GetUniformLocation(Program, "font");
            ColorsUniform = GlApi.GetUniformLocation(Program, "colors");
            ViewUniform = GlApi.GetUniformLocation(Program, "view");
            AlphaUniform = GlApi.GetUniformLocation(Program, "alpha");
            ScreenSizeUniform = GlApi.GetUniformLocation(Program, "screenSize");

            var (fontPixels, width, height) = LoadFont(assets);

            FontTexture = GlApi.GenTexture();
            GlApi.ActiveTexture(TextureUnit.Texture0);
            GlApi.BindTexture(TextureTarget.Texture2D, FontTexture);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GlApi.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.R8,
                width, height,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                fontPixels.AsSpan());

            ScreenTexture = GlApi.GenTexture();
            GlApi.ActiveTexture(TextureUnit.Texture1);
            GlApi.BindTexture(TextureTarget.Texture2D, ScreenTexture);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GlApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GlApi.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rg8,
                screen.Width, screen.Height,
                PixelFormat.Rg,
                PixelType.UnsignedByte,
                screen.AsSpan());

            ColorsTexture = GlApi.GenTexture();
            GlApi.ActiveTexture(TextureUnit.Texture2);
            GlApi.BindTexture(TextureTarget.Texture1D, ColorsTexture);
            GlApi.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GlApi.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GlApi.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GlApi.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            var colors =
                (
                    from rgb in new byte[] { 0x00, 0x3F, 0x7F, 0xBF }
                    select new Rgba32(rgb, rgb, rgb, 255)
                )
                .Concat(
                    from r in new byte[] { 0xFF, 0x7F }
                    from g in new byte[] { 0xFF, 0xAA, 0x55 }
                    from b in new byte[] { 0xFF, 0x7F }
                    select new Rgba32(r, g, b, 255)
                )
                .ToArray();

            GlApi.TexImage1D(
                TextureTarget.Texture1D,
                0,
                PixelInternalFormat.Rgba8,
                16,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                colors.AsSpan());
        }

        private static (byte[] pixels, int width, int height) LoadFont(IAssetProvider assets)
        {
            using var stream = assets.OpenRead("Images", "ascii.png");
            var image = Image.Load<Rgba32>(stream);
            var frame = image.Frames[0];
            var (width, height) = (frame.Width, frame.Height);
            var fontPixels = new byte[width * height];
            var pos = 0;
            for (var y = 0; y < height; ++y)
                for (var x = 0; x < width; ++x)
                    fontPixels[pos++] = frame.PixelBuffer[x, y].R;
            return (fontPixels, width, height);
        }

        public void Update()
        {
            var span = Screen.AsInvalidSpan(out int xOffset, out int yOffset, out int width, out int height);
            if (span.Length == 0)
                return;

            Screen.ClearInvalid();

            GlApi.ActiveTexture(TextureUnit.Texture1);
            GlApi.BindTexture(TextureTarget.Texture2D, ScreenTexture);

            GlApi.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                xOffset, yOffset,
                width, height,
                PixelFormat.Rg,
                PixelType.UnsignedByte,
                span);
        }

        public void Draw(Matrix4 view)
        {
            GlApi.UseProgram(Program);
            GlApi.BindVertexArray(Painter.Vao);

            GlApi.ActiveTexture(TextureUnit.Texture0);
            GlApi.BindTexture(TextureTarget.Texture2D, FontTexture);
            GlApi.Uniform1(FontUniform, 0);

            GlApi.ActiveTexture(TextureUnit.Texture1);
            GlApi.BindTexture(TextureTarget.Texture2D, ScreenTexture);
            GlApi.Uniform1(ScreenUniform, 1);

            GlApi.ActiveTexture(TextureUnit.Texture2);
            GlApi.BindTexture(TextureTarget.Texture1D, ColorsTexture);
            GlApi.Uniform1(ColorsUniform, 2);

            GlApi.Uniform1(AlphaUniform, Alpha);
            GlApi.Uniform2(ScreenSizeUniform, new Vector2(Screen.Width, Screen.Height));

            GlApi.UniformMatrix4(ViewUniform, true, ref view);
            GlApi.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedByte, 0);
        }
    }
}
