
namespace Weasyprint.Wrapped;

public class VersionResult
{
    public VersionResult(string version, string error, TimeSpan runTime, int exitCode)
    {
        this.Version = version;
        this.Error = error;
        this.RunTime = runTime;
        this.ExitCode = exitCode;
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public string Version { get; }
    public string Error { get; }
    public TimeSpan RunTime { get; }
    public int ExitCode { get; }
}