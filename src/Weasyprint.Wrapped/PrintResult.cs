namespace Weasyprint.Wrapped;

public class PrintResult : PrintBaseResult
{
    public PrintResult(byte[] bytes, string error, TimeSpan runTime, int exitCode)
        : base(error, runTime, exitCode)
    {
        Bytes = bytes;
    }

    public byte[] Bytes { get; }
}