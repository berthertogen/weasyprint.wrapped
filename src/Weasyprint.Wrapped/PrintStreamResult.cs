namespace Weasyprint.Wrapped;

public class PrintStreamResult : PrintBaseResult
{
    public PrintStreamResult(Stream documentDocumentStream, string error, TimeSpan runTime, int exitCode)
        : base(error, runTime, exitCode)
    {
        DocumentStream = documentDocumentStream;
    }

    public Stream DocumentStream { get; }
}