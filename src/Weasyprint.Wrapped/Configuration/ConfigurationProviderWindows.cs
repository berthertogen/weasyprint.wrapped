namespace Weasyprint.Wrapped;
public class ConfigurationProviderWindows : IConfigurationProvider
{
    public ConfigurationProviderWindows() : base() { }
    public ConfigurationProviderWindows(string workingFolder) : base(workingFolder) { }

    public override string GetAsset()
    {
        return Path.Combine(binFolder, "./standalone-windows-64.zip");
    }
}
