using System.Runtime.InteropServices;
using System.Text;
using CliWrap;

namespace Weasyprint.Wrapped;
public class CliRunner
{
    protected StringBuilder stdOutBuffer = new StringBuilder();
    protected StringBuilder stdErrBuffer = new StringBuilder();
    protected readonly string inputFile;
    protected readonly string outputFile;
    protected Command command;

    public CommandResult Result { get; set; }

    public CliRunner(ConfigurationProvider configurationProvider)
    {
        var cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{configurationProvider.GetWorkingFolder()}/python/python.exe" : "python3";
        var workingFolderEnd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : "bin";
        command = Cli
            .Wrap(cmd)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithWorkingDirectory(Path.Combine($"{configurationProvider.GetWorkingFolder()}/python", workingFolderEnd))
            .WithValidation(CommandResultValidation.None);
        inputFile = Path.Combine(configurationProvider.GetWorkingFolder(), $"{Guid.NewGuid()}.html");
        outputFile = Path.Combine(configurationProvider.GetWorkingFolder(), $"{Guid.NewGuid()}.pdf");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var path = $"{Environment.GetEnvironmentVariable("PATH")};{new FileInfo($"{configurationProvider.GetWorkingFolder()}/gtk3/bin").FullName}";
            command = command.WithEnvironmentVariables(env => env.Set("PATH", path));
        }
    }


    public void AddInput(string html)
    {
        File.WriteAllText(inputFile, html);
        command = command
            .WithArguments($"-m weasyprint {inputFile} {outputFile} -e utf8");
    }

    public async Task<byte[]> ExecuteAsync()
    {
        Result = await this.command.ExecuteAsync();
        if (File.Exists(outputFile))
        {
            return await File.ReadAllBytesAsync(outputFile);
        }
        else
        {
            return new byte[] { };
        }
    }
}
