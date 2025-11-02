using CliWrap;

namespace Weasyprint.Wrapped;

public class InitializeException : Exception
{
    public InitializeException(CommandResult result, string errorOutput) : base(@$"Error happened during weasyprint initialization
            ErrorOutput: {errorOutput}
            ExitCode: {result.ExitCode}
        ")
    {
    }
}