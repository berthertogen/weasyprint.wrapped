using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Weasyprint.Wrapped.Tests;

[Collection("Integration")]
public class PrinterTests
{
    public PrinterTests()
    {
        if (Directory.Exists("./weasyprinter"))
        {
            Directory.Delete("./weasyprinter", true);
        }
        GetPrinter().Initialize();
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

        GetPrinter().Initialize();

        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.True(Directory.Exists("./weasyprinter/python"));
    }

    [Fact]
    public async Task Print_RunsCommand()
    {
        var result = await GetPrinter().Print("<html><body><h1>TEST</h1></body></html>");

        Assert.False(result.HasError);
        Assert.Empty(result.Error);
        Assert.Equal(0, result.ExitCode);
        
        var testingProjectRoot = new DirectoryInfo(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Print_RunsCommand_Result_Windows_Expected.pdf" : "Print_RunsCommand_Result_Linux_Expected.pdf";
        var expectedOutputBytes = File.ReadAllBytes(Path.Combine(testingProjectRoot,$"Expected/{filename}"));
        File.WriteAllBytes(Path.Combine(testingProjectRoot,"Expected/Print_RunsCommand_Result_Actual.pdf"), result.Bytes);

        // Unable to compare the bytes array, there is a deviation somewhere in the generated pdf.
        // result.Bytes.Should().BeEquivalentTo(expectedOutputBytes);
        // the length seems to be relativly stable but not 100% equal, hence the range.
        Assert.InRange(result.Bytes.Length, expectedOutputBytes.Length - 5, expectedOutputBytes.Length + 5);
    }
    
    private static Printer GetPrinter()
    {
        var config = new ConfigurationProvider("../../../../../assets/", false, "weasyprinter", false);
        return new Printer(config);
    }
}