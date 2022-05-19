using CliWrap;

namespace Weasyprint.Wrapped.Tests;

public class StubCliRunner : CliRunner
{
    public StubCliRunner() : base(new StubConfigurationProvider()) { }

    public Command Command => this.command;
    public string Errors => this.stdErrBuffer.ToString();
    public string Output => this.stdOutBuffer.ToString();
    public string OutputFile => outputFile;
}
