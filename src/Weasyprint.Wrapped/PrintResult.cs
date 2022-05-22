
namespace Weasyprint.Wrapped;

public class PrintResult
{
    public PrintResult(byte[] bytes, string error, TimeSpan runTime, int exitCode)
    {
        this.Bytes = bytes;
        this.Error = error;
        this.RunTime = runTime;
        this.ExitCode = exitCode;
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public byte[] Bytes { get; }
    public string Error { get; }
    public TimeSpan RunTime { get; }
    public int ExitCode { get; }
}