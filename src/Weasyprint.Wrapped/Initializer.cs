using System.IO.Compression;

namespace Weasyprint.Wrapped;
public class Initializer
{
    private readonly IConfigurationProvider assetProvider;

    public Initializer(IConfigurationProvider assetProvider)
    {
        this.assetProvider = assetProvider;
    }

    public void Do()
    {
        if (Directory.Exists(assetProvider.GetWorkingFolder()))
        {
            return;
        }
        Directory.CreateDirectory(assetProvider.GetWorkingFolder());
        ZipFile.ExtractToDirectory(assetProvider.GetAsset(), assetProvider.GetWorkingFolder());
    }
}
