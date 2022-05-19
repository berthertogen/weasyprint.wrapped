namespace Weasyprint.Wrapped.Tests;

public class StubConfigurationProviderWindows : IConfigurationProvider
{
    public StubConfigurationProviderWindows() : base("./weasyprinter") { }

    public override string GetAsset()
    {
        return $"../../../../../assets/standalone-windows-64.zip";
    }
}
