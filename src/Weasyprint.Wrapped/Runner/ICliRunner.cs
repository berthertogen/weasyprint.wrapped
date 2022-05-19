using System.Text;
using CliWrap;

namespace Weasyprint.Wrapped;
public abstract class ICliRunner
{
    protected StringBuilder stdOutBuffer = new StringBuilder();
    protected StringBuilder stdErrBuffer = new StringBuilder();
    protected readonly string inputFile;
    protected readonly string outputFile;
    protected Command command;

    public CommandResult Result { get; set; }

    public ICliRunner(IConfigurationProvider configurationProvider, string cmd)
    {
        command = Cli
            .Wrap(cmd)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None);
        inputFile = Path.Combine(configurationProvider.GetWorkingFolder(),$"{Guid.NewGuid()}.html");
        outputFile = Path.Combine(configurationProvider.GetWorkingFolder(),$"{Guid.NewGuid()}.pdf");
    }


    public void AddInput(string html)
    {
        File.WriteAllText(inputFile, html);
        command = command
            .WithArguments($"-m weasyprint {inputFile} {outputFile} -e utf8");
    }

    public async Task<byte[]> ExecuteAsync() {
        Result = await this.command.ExecuteAsync();
        return await File.ReadAllBytesAsync(outputFile);
    }
}
