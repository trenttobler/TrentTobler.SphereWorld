using NUnit.Framework;

namespace TrentTobler.RetroCog;

public class FileAssetProviderTest
{
    private FileAssetProvider Instance { get; } = new FileAssetProvider { RootFolder = "TestData/FileAssetProvider" };

    [Test]
    public void Singleton_Should_HaveDefaultProperties()
    {
        Assert.AreEqual("Assets", FileAssetProvider.Instance.RootFolder, "RootFolder");
    }

    [TestCase("", "one.txt", "three.json", "two.txt")]
    [TestCase("Subfolder", "sub.two", "sub1")]
    public void ListFiles_Should_IncludeAllFileNames(string folder, params string[] want)
        => CollectionAssert.AreEquivalent(want, Instance.ListFiles(folder));

    [TestCase("", "one.txt", "test file one\nsecond line in the first file\n")]
    [TestCase("Subfolder", "sub1", "1")]
    public void LoadString_Should_LoadTextFile(string folder, string path, string want)
        => Assert.AreEqual(want, Instance.LoadString(folder, path));
}
