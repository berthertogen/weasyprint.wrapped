using System.Runtime.InteropServices;

namespace Weasyprint.Wrapped;
public class ConfigurationProvider
{
    protected readonly string binFolder;
    protected string workingFolder;

    public ConfigurationProvider() : this("weasyprinter") { }

    public ConfigurationProvider(string workingFolder)
    {
        binFolder = AppContext.BaseDirectory;
        if (Path.IsPathFullyQualified(workingFolder))
        {
            this.workingFolder = workingFolder;
        }
        else
        {
            this.workingFolder = Path.Combine(AppContext.BaseDirectory, workingFolder);
        }
    }

    public virtual string GetAsset() {
        var env = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";
        return Path.Combine(binFolder, $"standalone-{env}-64.zip");
    }

    public string GetWorkingFolder()
        => workingFolder;
}
