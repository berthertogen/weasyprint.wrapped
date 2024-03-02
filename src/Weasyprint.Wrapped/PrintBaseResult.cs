
namespace Weasyprint.Wrapped;

public abstract class PrintBaseResult
{
    public PrintBaseResult(string error, TimeSpan runTime, int exitCode)
    {
        this.Error = error;
        this.RunTime = runTime;
        this.ExitCode = exitCode;
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);
    
    public string Error { get; }
    public TimeSpan RunTime { get; }
    public int ExitCode { get; }
}