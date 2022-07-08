using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using CliWrap;
using CliWrap.Buffered;

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

    public async Task Initialize()
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var stdErrBuffer = new StringBuilder();
            var command = await Cli
                .Wrap("/bin/bash")
                .WithArguments(a =>
                {
                    a.Add("-c");
                    a.Add("chmod -R 775 .");
                })
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .WithWorkingDirectory($"{workingFolder}")
                .ExecuteAsync();
            if (stdErrBuffer.Length > 0)
            {
                throw new InitializeException(command, stdErrBuffer.ToString());
            }
        }
    }

    public async Task<PrintResult> Print(string html)
    {
        using var outputStream = new MemoryStream();
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
            .WithArguments($"-m weasyprint - - --encoding utf8")
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .WithStandardInputPipe(PipeSource.FromString(html, Encoding.UTF8))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8);
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
                .Wrap($"{workingFolder}/python/bin/python3.10")
                .WithWorkingDirectory($"{workingFolder}/python/bin/");
        }
        return command;
    }

    public async Task<VersionResult> Version()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
            .WithArguments($"-m weasyprint --info")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8);
        return new VersionResult(
            stdOutBuffer.ToString(),
            stdErrBuffer.ToString(),
            result.RunTime,
            result.ExitCode
        );
    }
}
