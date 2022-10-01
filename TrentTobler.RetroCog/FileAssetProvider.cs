namespace TrentTobler.RetroCog;

public class FileAssetProvider : IAssetProvider
{
    public static FileAssetProvider Instance { get; } = new FileAssetProvider();

    public string RootFolder { get; set; } = "Assets";

    public IEnumerable<string> ListFiles(string folder) => Directory
        .EnumerateFiles(Path.Combine(RootFolder, folder))
        .Select(fullName => Path.GetFileName(fullName));

    public Stream OpenRead(string folder, string file) => File
        .OpenRead(Path.Combine(RootFolder, folder, file));
}
