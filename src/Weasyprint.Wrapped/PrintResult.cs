
namespace Weasyprint.Wrapped;

public class PrintResult : PrintBaseResult
{
    public PrintResult(byte[] bytes, string error, TimeSpan runTime, int exitCode)
        : base(error, runTime, exitCode)
    {
        this.Bytes = bytes;
    }

    public byte[] Bytes { get; }
}