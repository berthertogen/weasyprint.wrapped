
namespace Weasyprint.Wrapped;

public class PrintResult
{
    public PrintResult(byte[] bytes, string error, string output, TimeSpan runTime, int exitCode, string tempOutputFile)
    {
        this.Bytes = bytes;
        this.Error = error;
        this.Output = output;
        this.RunTime = runTime;
        this.ExitCode = exitCode;
        this.TempOutputFile = tempOutputFile;
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public byte[] Bytes { get; }
    public string Error { get; }
    public string Output { get; }
    public TimeSpan RunTime { get; }
    public int ExitCode { get; }
    public string TempOutputFile { get; }
}