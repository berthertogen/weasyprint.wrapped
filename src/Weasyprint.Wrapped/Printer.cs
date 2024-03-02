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
    private readonly string baseUrl;

    public Printer()
        : this(new ConfigurationProvider()) {}

    public Printer(ConfigurationProvider configurationProvider)
    {
        workingFolder = configurationProvider.GetWorkingFolder();
        asset         = configurationProvider.GetAsset();
        baseUrl       = configurationProvider.GetBaseUrl();
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

    /// <summary>
    /// Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <param name="html">html content to be converted to pdf</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintResult> Print(string html, params string[] additionalParameters)
    {
        return await Print(html, default, additionalParameters);
    }

    /// <summary>
    /// Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <returns>A result with a byte array containing the generated pdf</returns>
    /// <param name="html">html content to be converted to pdf</param>
    /// <param name="cancellationToken">Optional cancellationToken, passed to the executing command</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintResult> Print(string html, CancellationToken cancellationToken = default, params string[] additionalParameters)
    {
        var streamResult = await PrintStream(html, cancellationToken, additionalParameters);
        
        var documentBytes = (streamResult.DocumentStream as MemoryStream)?.ToArray() ?? [];
        streamResult.DocumentStream.Dispose();
        
        return new PrintResult(
                               documentBytes,
                               streamResult.Error,
                               streamResult.RunTime,
                               streamResult.ExitCode
                              );
    }

    /// <summary>
    /// Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <returns>A result with an open stream containing the generated pdf document</returns>
    /// <param name="html">html content to be converted to pdf</param>
    /// <param name="cancellationToken">Optional cancellationToken, passed to the executing command</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintStreamResult> PrintStream(string html, CancellationToken cancellationToken = default, params string[] additionalParameters)
    {
        var outputStream = new MemoryStream();
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
                          .WithArguments($"-m weasyprint - - --encoding utf8 --base-url {baseUrl} {string.Join(" ", additionalParameters)}")
                          .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
                          .WithStandardInputPipe(PipeSource.FromString(html, Encoding.UTF8))
                          .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                          .WithValidation(CommandResultValidation.None)
                          .ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);

        outputStream.Seek(0, SeekOrigin.Begin);
        
        return new PrintStreamResult(
                                     outputStream,
                                     stdErrBuffer.ToString(),
                                     result.RunTime,
                                     result.ExitCode
                                    );
    }

    /// <summary>
    /// Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <param name="htmlFile">html file name with path to be converted to pdf</param>
    /// <param name="pdfFile">pdf file name with path for output</param>
    /// <param name="cancellationToken">Optional cancellationToken, passed to the executing command</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintResult> Print(string htmlFile, string pdfFile, CancellationToken cancellationToken = default, params string[] additionalParameters)
    {
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
                          .WithArguments($"-m weasyprint --encoding utf8 --base-url {baseUrl} {string.Join(" ", additionalParameters)} {htmlFile} {pdfFile}")
                          .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                          .WithValidation(CommandResultValidation.None)
                          .ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);

        return new PrintResult(
                               Array.Empty<byte>(),
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
                     .WithEnvironmentVariables(env => env.Set("PATH",
                                                              $"{new FileInfo($"{workingFolder}/gtk3").FullName};{Environment.GetEnvironmentVariable("PATH")}"));
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