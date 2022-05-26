using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder()
    {
       await GetPrinter().Initialize();

        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.True(Directory.Exists("./weasyprinter/python"));
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder_DeletesFolderIfExistsAndNoVersionInfo()
    {
        Directory.CreateDirectory("./weasyprinter");

        var timeBeforeAction = DateTime.Now;
        await GetPrinter().Initialize();

        var creationTime = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedAfter = creationTime.TimeOfDay > timeBeforeAction.TimeOfDay;
        Assert.True(isCreatedAfter, $"Should be created ({creationTime.ToLongTimeString()}) after {timeBeforeAction.ToLongTimeString()}");
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder_DeletesFolderIfVersionIsDifferent()
    {
        Directory.CreateDirectory("./weasyprinter");
        var fileStream = File.Create($"./weasyprinter/version-somethingdifferent");
        fileStream.Close();

        var timeBeforeAction = DateTime.Now;
        await GetPrinter().Initialize();

        var creationTime = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedAfter = creationTime.TimeOfDay > timeBeforeAction.TimeOfDay;
        Assert.True(isCreatedAfter, $"Should be created ({creationTime.ToLongTimeString()}) after {timeBeforeAction.ToLongTimeString()}");
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder_LeaveFolderIfVersionIsSame()
    {
        Directory.CreateDirectory("./weasyprinter");
        var env = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";
        var asset = ZipFile.OpenRead($"../../../../../assets/standalone-{env}-64.zip");
        var version = asset.Entries.Single(e => e.Name.StartsWith("version-")).Name;
        var fileStream = File.Create($"./weasyprinter/{version}");
        fileStream.Close();

        var timeBeforeAction = DateTime.Now;
        await GetPrinter().Initialize();

        var creationTime = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedBefore = creationTime.TimeOfDay < timeBeforeAction.TimeOfDay;
        Assert.True(isCreatedBefore, $"Should be created ({creationTime.ToLongTimeString()}) before {timeBeforeAction.ToLongTimeString()}");
    }

    [Fact]
    public async Task Print_RunsCommand_Simple()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var result = await printer.Print("<html><body><h1>TEST</h1></body></html>");

        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);

        var testingProjectRoot = new DirectoryInfo(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Print_RunsCommand_Result_Windows_Expected.pdf" : "Print_RunsCommand_Result_Linux_Expected.pdf";
        var expectedOutputBytes = File.ReadAllBytes(Path.Combine(testingProjectRoot, $"Expected/{filename}"));
        File.WriteAllBytes(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_Result_Actual.pdf"), result.Bytes);
        Assert.True(result.Bytes.Length > 0);
    }

    [Fact]
    public async Task Print_RunsCommand_SpecialCharacters()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var testingProjectRoot = new DirectoryInfo(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        var html = File.ReadAllText(Path.Combine(testingProjectRoot,"Expected/Print_RunsCommand_SpecialCharacters_Input.html"), System.Text.Encoding.UTF8);
        var result = await printer.Print(html);

        File.WriteAllBytes(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_SpecialCharacters_Output.pdf"), result.Bytes);

        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);
        Assert.True(result.Bytes.Length > 0);
    }

    private static Printer GetPrinter()
    {
        var config = new ConfigurationProvider("../../../../../assets/", false, "weasyprinter", false);
        return new Printer(config);
    }
}