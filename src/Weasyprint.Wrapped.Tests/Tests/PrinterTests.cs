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
        new StubPrinter().Initialize();
    }

    [Fact]
    public void Initialize_UnzipsAssetToFolder()
    {
        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.True(Directory.Exists("./weasyprinter/python"));
    }

    [Fact]
    public void Initialize_UnzipsAssetToFolder_DeletesFolderIfExists()
    {
        Directory.CreateDirectory("./weasyprinter");

        new StubPrinter().Initialize();

        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.True(Directory.Exists("./weasyprinter/python"));
    }

    [Fact]
    public async Task Print_RunsCommand()
    {
        var stub = new StubPrinter();

        var result = await stub.Print("<html><body><h1>TEST</h1></body></html>");

        Assert.False(result.HasError);
        Assert.Empty(result.Error);
        Assert.True(File.Exists(result.TempOutputFile));
        var outputBytes = File.ReadAllBytes(result.TempOutputFile);
        Assert.Equal(outputBytes, result.Bytes);
        Assert.Equal(0, result.ExitCode);
    }
}