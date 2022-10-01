namespace TrentTobler.RetroCog.Graphics
{
    public class ImageLoader
    {
        private IAssetProvider Assets { get; }

        public ImageLoader(IAssetProvider assets)
        {
            Assets = assets;
        }

        /* TODO: fix this
        public byte[] Load(string path, string file)
        {
            using var stream = Assets.OpenRead(path, file);
            var image = Image.Load<Rgba32>(stream);
            var frame = image.Frames[0];
            var (width, height) = (frame.Width, frame.Height);
            var texture = new byte[width*height];
            var pos = 0;
            for(var y = 0; y < height; ++y)
                for(var x = 0; x < width; ++x)
                    texture[pos++] = frame.PixelBuffer[x, y].R;
            return texture;
        }
        */
    }
}
