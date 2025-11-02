using System.Runtime.InteropServices;

namespace Weasyprint.Wrapped;

public class ConfigurationProvider
{
    protected readonly string assetsFolder;
    protected readonly string workingFolder;
    private string baseUrl = ".";

    public ConfigurationProvider() : this(string.Empty, false, "weasyprinter", false)
    {
    }

    public ConfigurationProvider(string assetsFolder, bool isAssetsAbsolute, string workingFolder, bool isWorkingAbsolute)
    {
        this.assetsFolder = GetFolder(assetsFolder, isAssetsAbsolute);
        this.workingFolder = GetFolder(workingFolder, isWorkingAbsolute);
    }

    private string GetFolder(string folder, bool isAbsolute)
    {
        return isAbsolute ? folder : Path.Combine(AppContext.BaseDirectory, folder);
    }

    public string GetAsset()
    {
        var env = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";
        return Path.Combine(assetsFolder, $"standalone-{env}-64.zip");
    }

    public string GetWorkingFolder()
    {
        return workingFolder;
    }

    public void SetBaseUrl(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public string GetBaseUrl()
    {
        return baseUrl;
    }
}