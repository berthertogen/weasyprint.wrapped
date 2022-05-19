using CliWrap;

namespace Weasyprint.Wrapped.Tests;

public class StubCliRunnerWindows : CliRunnerWindows
{
    public StubCliRunnerWindows() : base(new StubConfigurationProviderWindows()) { }

    public Command Command => this.command;
    public string Errors => this.stdErrBuffer.ToString();
    public string Output => this.stdOutBuffer.ToString();
    public string OutputFile => outputFile;
}
