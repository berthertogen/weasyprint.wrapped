using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using CliWrap;

namespace Weasyprint.Wrapped;
public class Printer
{
    protected StringBuilder stdOutBuffer = new StringBuilder();
    protected StringBuilder stdErrBuffer = new StringBuilder();
    protected Command command;
    private readonly string workingFolder;
    private readonly string asset;

    public Printer() : this(new ConfigurationProvider()) { }

    public Printer(ConfigurationProvider configurationProvider)
    {
        workingFolder = configurationProvider.GetWorkingFolder();
        asset = configurationProvider.GetAsset();
        var cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{workingFolder}/python/python.exe" : "python3";
        var workingFolderEnd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : "bin";
        command = Cli
            .Wrap(cmd)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithWorkingDirectory(Path.Combine($"{workingFolder}/python", workingFolderEnd))
            .WithValidation(CommandResultValidation.None);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var path = $"{Environment.GetEnvironmentVariable("PATH")};{new FileInfo($"{workingFolder}/gtk3").FullName}";
            command = command.WithEnvironmentVariables(env => env.Set("PATH", path));
        }
    }

    public void Initialize()
    {
        if (Directory.Exists(workingFolder))
        {
            Directory.Delete(workingFolder, true);
        }
        Directory.CreateDirectory(workingFolder);
        ZipFile.ExtractToDirectory(asset, workingFolder);
    }

    public async Task<PrintResult> Print(string html)
    {
        var inputFile = Path.Combine(workingFolder, $"{Guid.NewGuid()}.html");
        var outputFile = Path.Combine(workingFolder, $"{Guid.NewGuid()}.pdf");
        File.WriteAllText(inputFile, html);
        command = command.WithArguments($"-m weasyprint {inputFile} {outputFile} -e utf8");
        var result = await this.command.ExecuteAsync();
        return new PrintResult(
            File.Exists(outputFile) ? File.ReadAllBytes(outputFile) : Array.Empty<byte>(),
            stdErrBuffer.ToString(),
            stdOutBuffer.ToString(),
            result.RunTime,
            result.ExitCode,
            outputFile
        );
    }
}
