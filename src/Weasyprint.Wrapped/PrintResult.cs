
namespace Weasyprint.Wrapped;

public readonly record struct PrintResult(byte[] Bytes, string Error, string Output, TimeSpan RunTime, int ExitCode, string TempOutputFile) {
    public bool HasError => !string.IsNullOrWhiteSpace(Error);
};