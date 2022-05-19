using System.Runtime.InteropServices;

namespace Weasyprint.Wrapped.Tests;

public class StubConfigurationProvider : ConfigurationProvider
{
    public StubConfigurationProvider() : base("weasyprinter") { }

    public override string GetAsset()
    {
        var env = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";
        return $"../../../../../assets/standalone-{env}-64.zip";
    }
}
