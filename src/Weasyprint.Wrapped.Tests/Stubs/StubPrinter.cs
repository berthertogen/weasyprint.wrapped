using CliWrap;

namespace Weasyprint.Wrapped.Tests;

public class StubPrinter : Printer
{
    public StubPrinter() : base(new StubConfigurationProvider()) { }

    public Command Command => this.command;
    public string Errors => this.stdErrBuffer.ToString();
    public string Output => this.stdOutBuffer.ToString();
}
