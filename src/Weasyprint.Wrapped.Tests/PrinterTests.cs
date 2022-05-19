using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Weasyprint.Wrapped.Tests;

[Collection("Integration")]
public class PrinterTests
{
    public PrinterTests()
    {
        if (Directory.Exists("./weasyprinter")) {
            Directory.Delete("./weasyprinter", true);
        }
        new Initializer(new StubConfigurationProvider()).Do();
    }

    [Fact]
    public async Task Do_RunsCommandUnderWindows()
    {
        var stub = new StubCliRunner();

        var result = await new Printer(stub).Do("<html><body><h1>TEST</h1></body></html>");

        Assert.Empty(stub.Errors);
        Assert.True(File.Exists(stub.OutputFile));
        var outputBytes = File.ReadAllBytes(stub.OutputFile);
        Assert.Equal(outputBytes, result);
        Assert.Equal(0, stub.Result.ExitCode);
    }
}