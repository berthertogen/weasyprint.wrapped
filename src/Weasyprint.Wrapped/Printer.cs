using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using CliWrap;

namespace Weasyprint.Wrapped;
public class Printer
{
    private readonly string workingFolder;
    private readonly string asset;

    public Printer() : this(new ConfigurationProvider()) { }

    public Printer(ConfigurationProvider configurationProvider)
    {
        workingFolder = configurationProvider.GetWorkingFolder();
        asset = configurationProvider.GetAsset();
    }

    public void Initialize()
    {
        var version = ZipFile.OpenRead(asset).Entries.Single(e => e.Name.StartsWith("version-")).Name;
        if (File.Exists(Path.Combine(workingFolder, version)))
        {
            return;
        }
        if (Directory.Exists(workingFolder))
        {
            Directory.Delete(workingFolder, true);
        }
        Directory.CreateDirectory(workingFolder);
        ZipFile.ExtractToDirectory(asset, workingFolder);
    }

    public async Task<PrintResult> Print(string html)
    {
        using var outputStream = new MemoryStream();
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
            .WithArguments($"-m weasyprint - - -e utf8")
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .WithStandardInputPipe(PipeSource.FromString(html))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
        return new PrintResult(
            outputStream.ToArray(),
            stdErrBuffer.ToString(),
            result.RunTime,
            result.ExitCode
        );
    }

    private Command BuildOsSpecificCommand()
    {
        Command command;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = Cli
                .Wrap($"{workingFolder}/python/python.exe")
                .WithWorkingDirectory($"{workingFolder}/python")
                .WithEnvironmentVariables(env => env.Set("PATH", $"{Environment.GetEnvironmentVariable("PATH")};{new FileInfo($"{workingFolder}/gtk3").FullName}"));
        }
        else
        {
            command = Cli
                .Wrap("python3")
                .WithWorkingDirectory($"{workingFolder}/python/bin");
        }
        return command;
    }

}
