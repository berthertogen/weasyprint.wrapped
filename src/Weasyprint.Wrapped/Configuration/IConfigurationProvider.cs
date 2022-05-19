namespace Weasyprint.Wrapped;
public abstract class IConfigurationProvider
{
    protected readonly string binFolder;
    protected string workingFolder;

    public IConfigurationProvider() : this("./weasyprinter") { }

    public IConfigurationProvider(string workingFolder)
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

    public abstract string GetAsset();

    public string GetWorkingFolder()
        => workingFolder;
}
