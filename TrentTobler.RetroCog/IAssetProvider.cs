namespace TrentTobler.RetroCog;

public interface IAssetProvider
{
    IEnumerable<string> ListFiles(string path);
    Stream OpenRead(string path, string file);
}

public static class AssetProviderExtensions
{
    public static string LoadString(this IAssetProvider assets, string folder, string file)
    {
        using var reader = assets.OpenText(folder, file);
        return reader.ReadToEnd();
    }

    public static TextReader OpenText(this IAssetProvider assets, string path, string file)
        => new StreamReader(assets.OpenRead(path, file));
}
