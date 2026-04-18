using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Weasyprint.Wrapped.Tests;

[Collection("Integration")]
public class PrinterTests
{
    private readonly string testingProjectRoot = new DirectoryInfo(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

    public PrinterTests()
    {
        if (Directory.Exists("./weasyprinter")) Directory.Delete("./weasyprinter", true);
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder()
    {
        await GetPrinter().Initialize();

        Assert.True(Directory.Exists("./weasyprinter"));
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder_DeletesFolderIfExistsAndNoVersionInfo()
    {
        Directory.CreateDirectory("./weasyprinter");

        var creationTimeBeforeAction = new DirectoryInfo("./weasyprinter").CreationTime;
        await Task.Delay(10);
        await GetPrinter().Initialize();

        var creationTimeAfterAction = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedAfter = creationTimeAfterAction > creationTimeBeforeAction;
        Assert.True(isCreatedAfter, $"Should be created ({creationTimeAfterAction:HH:mm:ss.fff}) after {creationTimeBeforeAction:HH:mm:ss.fff}");
    }

    [Fact]
    public async Task Initialize_UnzipsAssetToFolder_DeletesFolderIfVersionIsDifferent()
    {
        Directory.CreateDirectory("./weasyprinter");
        var fileStream = File.Create("./weasyprinter/version-somethingdifferent");
        fileStream.Close();

        var creationTimeBeforeAction = new DirectoryInfo("./weasyprinter").CreationTime;
        await Task.Delay(10);
        await GetPrinter().Initialize();

        var creationTimeAfterAction = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedAfter = creationTimeAfterAction > creationTimeBeforeAction;
        Assert.True(isCreatedAfter, $"Should be created ({creationTimeAfterAction:HH:mm:ss.fff}) after {creationTimeBeforeAction:HH:mm:ss.fff}");
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

        var executableFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "weasyprint.exe" : "weasyprint";
        await File.WriteAllTextAsync(Path.Combine("./weasyprinter", executableFileName), "placeholder");

        var creationTimeBeforeAction = new DirectoryInfo("./weasyprinter").CreationTime;
        await Task.Delay(10);
        await GetPrinter().Initialize();

        var creationTimeAfterAction = new DirectoryInfo("./weasyprinter").CreationTime;
        var isCreatedBefore = creationTimeAfterAction == creationTimeBeforeAction;
        Assert.True(isCreatedBefore, $"Should be created ({creationTimeAfterAction:HH:mm:ss.fff}) before {creationTimeBeforeAction:HH:mm:ss.fff}");
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
        Assert.True(result.Bytes.Length > 0);
        AssertLooksLikePdf(result.Bytes);
    }

    [Fact]
    public async Task Print_RunsStreamCommand_Simple()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var result = await printer.PrintStream("<html><body><h1>TEST</h1></body></html>");

        var actualOutputBytes = (result.DocumentStream as MemoryStream)?.ToArray();

        Assert.NotNull(actualOutputBytes);
        Assert.True(actualOutputBytes.Length > 0);
        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);
        Assert.True(actualOutputBytes.Length > 0);
        AssertLooksLikePdf(actualOutputBytes);
    }

    [Fact]
    public async Task Print_RunsCommand_WithFilePaths_Simple()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var inputFile = Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_Simple_Input.html");
        var outputFile = Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_WithFilePaths_Result_Actual.pdf");
        var result = await printer.Print(inputFile, outputFile, CancellationToken.None);

        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);

        var outputFileBytes = await File.ReadAllBytesAsync(outputFile);
        Assert.True(outputFileBytes.Length > 0);
        AssertLooksLikePdf(outputFileBytes);
    }

    [Fact]
    public async Task Print_RunsCommand_WithParameters()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var html = await File.ReadAllTextAsync(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_SpecialCharacters_Input.html"), Encoding.UTF8);
        var resultNormal = await printer.Print(html);
        Assert.True(string.IsNullOrWhiteSpace(resultNormal.Error), $"Should have no error but found {resultNormal.Error}");
        Assert.Equal(0, resultNormal.ExitCode);
        Assert.False(resultNormal.HasError);
        var resultOptimized = await printer.Print(html, "--optimize-images");
        Assert.True(string.IsNullOrWhiteSpace(resultOptimized.Error), $"Should have no error but found {resultOptimized.Error}");
        Assert.Equal(0, resultOptimized.ExitCode);
        Assert.False(resultOptimized.HasError);

        Assert.True(resultNormal.Bytes.Length > resultOptimized.Bytes.Length, $"Expected {resultNormal.Bytes.Length} to be greater than {resultOptimized.Bytes.Length}");
        AssertLooksLikePdf(resultNormal.Bytes);
        AssertLooksLikePdf(resultOptimized.Bytes);
    }

    [Fact]
    public async Task Print_RunsStreamCommand_WithParameters()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var html = await File.ReadAllTextAsync(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_SpecialCharacters_Input.html"), Encoding.UTF8);

        var resultNormal = await printer.PrintStream(html);

        Assert.NotNull(resultNormal.DocumentStream);
        Assert.True(resultNormal.DocumentStream.Length > 0);
        Assert.True(string.IsNullOrWhiteSpace(resultNormal.Error), $"Should have no error but found {resultNormal.Error}");
        Assert.Equal(0, resultNormal.ExitCode);
        Assert.False(resultNormal.HasError);

        var resultOptimized = await printer.PrintStream(html, default, "--optimize-images");

        Assert.NotNull(resultOptimized.DocumentStream);
        Assert.True(resultOptimized.DocumentStream.Length > 0);
        Assert.True(string.IsNullOrWhiteSpace(resultOptimized.Error), $"Should have no error but found {resultOptimized.Error}");
        Assert.Equal(0, resultOptimized.ExitCode);
        Assert.False(resultOptimized.HasError);

        Assert.True(resultNormal.DocumentStream.Length > resultOptimized.DocumentStream.Length,
            $"Expected {resultNormal.DocumentStream.Length} to be greater than {resultOptimized.DocumentStream.Length}");
    }

    [Fact]
    public async Task Print_RunsCommand_SpecialCharacters()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var html = await File.ReadAllTextAsync(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_SpecialCharacters_Input.html"), Encoding.UTF8);
        var result = await printer.Print(html);

        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);
        Assert.True(result.Bytes.Length > 0);
        AssertLooksLikePdf(result.Bytes);
    }

    [Fact]
    public async Task Print_RunsStreamCommand_SpecialCharacters()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var html = await File.ReadAllTextAsync(Path.Combine(testingProjectRoot, "Expected/Print_RunsCommand_SpecialCharacters_Input.html"), Encoding.UTF8);
        var result = await printer.PrintStream(html);

        var actualBytes = (result.DocumentStream as MemoryStream)?.ToArray();

        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);
        Assert.True(result.DocumentStream.Length > 0);
        Assert.True(actualBytes?.Length > 0);
        AssertLooksLikePdf(actualBytes!);
    }

    [Fact]
    public async Task Initialize_ThrowsHelpfulError_WhenAssetMissing()
    {
        var missingAssetsFolder = Path.Combine(testingProjectRoot, "assets", "this-directory-does-not-exist");
        var config = new ConfigurationProvider(missingAssetsFolder, true, "weasyprinter", false);
        var printer = new Printer(config);

        var exception = await Assert.ThrowsAsync<InitializeException>(() => printer.Initialize());

        Assert.True(
            exception.Message.IndexOf("asset was not found", StringComparison.OrdinalIgnoreCase) >= 0,
            $"Unexpected error message: {exception.Message}");
    }

    [Fact]
    public async Task Version_ReturnsVersion()
    {
        var printer = GetPrinter();
        await printer.Initialize();

        var result = await printer.Version();

        Assert.Contains("WeasyPrint version: 68.1", result.Version);
        Assert.True(string.IsNullOrWhiteSpace(result.Error), $"Should have no error but found {result.Error}");
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.HasError);
    }

    private static Printer GetPrinter()
    {
        var config = new ConfigurationProvider("../../../../../assets/", false, "weasyprinter", false);
        return new Printer(config);
    }

    private static void AssertLooksLikePdf(byte[] bytes)
    {
        Assert.True(bytes.Length > 5, "Expected generated PDF bytes to have a valid length.");
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
        Assert.Equal((byte)'-', bytes[4]);
    }
}