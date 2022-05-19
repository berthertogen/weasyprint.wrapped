namespace Weasyprint.Wrapped;
public class CliRunnerWindows : ICliRunner
{
    public CliRunnerWindows(IConfigurationProvider configurationProvider) : base(configurationProvider, $"{configurationProvider.GetWorkingFolder()}/python/python.exe")
    {
        var path = $"{Environment.GetEnvironmentVariable("PATH")};{new FileInfo($"{configurationProvider.GetWorkingFolder()}/gtk3/bin").FullName}";
        command = command.WithEnvironmentVariables(env => env.Set("PATH", path));
    }
}
