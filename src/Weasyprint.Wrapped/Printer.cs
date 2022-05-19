namespace Weasyprint.Wrapped;
public class Printer
{
    private readonly CliRunner runner;

    public Printer(ConfigurationProvider assetProvider)
    {
        this.runner = new CliRunner(assetProvider);
    }
    public Printer(CliRunner runner)
    {
        this.runner = runner;
    }

    public async Task<byte[]> Print(string html)
    {
        runner.AddInput(html);
        return await runner.ExecuteAsync();
    }
}
