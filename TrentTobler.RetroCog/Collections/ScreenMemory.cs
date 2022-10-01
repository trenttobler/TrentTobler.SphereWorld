using System.Text;
using TrentTobler.RetroCog.Graphics;

namespace TrentTobler.RetroCog.Collections;

public class ScreenMemory : TextWriter, ScreenPainter.IScreen
{
    public override Encoding Encoding => Encoding.ASCII;

    public int Width { get; }
    public int Height { get; }

    public int TabWidth { get; set; } = 4;
    public int FgColor { get => ColorByte >> 4; set => SetColors(value, BgColor); }
    public int BgColor { get => ColorByte & 15; set => SetColors(FgColor, value); }

    private ScreenPainter.Symbol[] Symbols { get; }

    private (int startRow, int startCol, int width, int height) InvalidRegion { get; set; }
    private int CursorIndex { get; set; }
    private byte ColorByte { get; set; }

    public ScreenMemory(int width, int height)
    {
        Width = width;
        Height = height;
        Symbols = new ScreenPainter.Symbol[Width * Height];

        CursorIndex = 0;
    }

    public void SetColors(int fg, int bg)
        => ColorByte = (byte)((bg & 15) | fg << 4);

    public override void Write(char value)
    {
        if (value >= (char)32 && value <= (char)126)
        {
            var cursor = Math.DivRem(CursorIndex, Width);
            Invalidate(cursor);

            Symbols[CursorIndex++] = new ScreenPainter.Symbol
            {
                Token = (byte)(value - 32),
                Color = ColorByte,
            };

            FixCursor();
            return;
        }

        switch (value)
        {
            case '\r':
                CursorIndex -= CursorIndex % Width;
                return;

            case '\n':
                CursorIndex += Width - CursorIndex % Width;
                FixCursor();
                return;

            case '\t':
                CursorIndex += TabWidth - CursorIndex % TabWidth;
                FixCursor();
                return;

            case '\b':
                if (CursorIndex > 0)
                    --CursorIndex;
                return;
        }
    }

    public Span<ScreenPainter.Symbol> AsSpan()
        => Symbols.AsSpan();

    public Span<ScreenPainter.Symbol> AsInvalidSpan(out int xoffset, out int yoffset, out int width, out int height)
    {
        (yoffset, xoffset, width, height) = InvalidRegion;
        if ((height | width) == 0)
            return Symbols.AsSpan(0, 0);

        var index = GetSymbolIndex((yoffset, xoffset));
        var lastIndex = GetSymbolIndex((yoffset + height - 1, xoffset + width - 1));
        return Symbols.AsSpan(index, lastIndex - index + 1);
    }

    public void ClearInvalid()
        => InvalidRegion = (0, 0, 0, 0);

    private void ScrollUp()
    {
        const int lines = 1;
        var start = lines * Width;
        var len = (Height - lines) * Width;

        Symbols
            .AsSpan(start, len)
            .CopyTo(Symbols.AsSpan(0, len));

        Symbols
            .AsSpan(len, Symbols.Length - len)
            .Fill(new ScreenPainter.Symbol
            {
                Token = 0,
                Color = ColorByte,
            });

        InvalidRegion = (0, 0, Width, Height);
    }

    private void Invalidate((int row, int col) location)
    {
        if (InvalidRegion.width == 0)
        {
            InvalidRegion = (location.row, location.col, 1, 1);
            return;
        }

        var left = Math.Min(location.col, InvalidRegion.startCol);
        var top = Math.Min(location.row, InvalidRegion.startRow);
        var right = Math.Max(location.col + 1, InvalidRegion.startCol + InvalidRegion.width);
        var bottom = Math.Max(location.row + 1, InvalidRegion.startRow + InvalidRegion.height);

        if (top + 1 < bottom)
        {
            left = 0;
            right = Width;
        }

        InvalidRegion = (top, left, right - left, bottom - top);
    }

    private void FixCursor()
    {
        while (CursorIndex >= Symbols.Length)
        {
            ScrollUp();
            CursorIndex -= Width;
        }
    }

    private int GetSymbolIndex((int row, int col) value)
        => Width * Math.Min(Height - 1, Math.Max(0, value.row))
            + Math.Min(Width - 1, Math.Max(0, value.col));
}
