namespace Weasyprint.Wrapped;
public class Printer
{
    private readonly ICliRunner runner;

    public Printer(IConfigurationProvider assetProvider)
    {
        this.runner = new CliRunnerWindows(assetProvider);
    }
    public Printer(ICliRunner runner)
    {
        this.runner = runner;
    }

    public async Task<byte[]> Do(string html)
    {
        runner.AddInput(html);
        return await runner.ExecuteAsync();
    }
}
