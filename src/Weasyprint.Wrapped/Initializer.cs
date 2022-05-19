using System.IO.Compression;

namespace Weasyprint.Wrapped;
public class Initializer
{
    private readonly ConfigurationProvider assetProvider;

    public Initializer(ConfigurationProvider assetProvider)
    {
        this.assetProvider = assetProvider;
    }

    public void Initialize()
    {
        if (Directory.Exists(assetProvider.GetWorkingFolder()))
        {
            return;
        }
        Directory.CreateDirectory(assetProvider.GetWorkingFolder());
        ZipFile.ExtractToDirectory(assetProvider.GetAsset(), assetProvider.GetWorkingFolder());
    }
}
