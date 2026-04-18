using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using CliWrap;
using CliWrap.Buffered;

namespace Weasyprint.Wrapped;

public class Printer
{
    private readonly string asset;
    private readonly string baseUrl;
    private readonly string workingFolder;
    private readonly SemaphoreSlim initializeLock = new(1, 1);

    public Printer()
        : this(new ConfigurationProvider())
    {
    }

    public Printer(ConfigurationProvider configurationProvider)
    {
        workingFolder = configurationProvider.GetWorkingFolder();
        asset = configurationProvider.GetAsset();
        baseUrl = configurationProvider.GetBaseUrl();
    }

    public async Task Initialize()
    {
        await initializeLock.WaitAsync();
        try
        {
            if (!File.Exists(asset))
                throw new InitializeException($"Weasyprint asset was not found at '{asset}'. Ensure the standalone zip is copied to output (contentFiles/any/any).");

            string version;
            try
            {
                using var zipArchive = ZipFile.OpenRead(asset);
                var versionEntries = zipArchive.Entries
                    .Where(e => e.Name.StartsWith("version-", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (versionEntries.Length != 1)
                    throw new InitializeException($"Expected exactly one version marker entry (version-*) in '{asset}', but found {versionEntries.Length}.");

                version = versionEntries[0];
            }
            catch (InitializeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InitializeException($"Failed reading weasyprint asset '{asset}'.", ex);
            }

            if (File.Exists(Path.Combine(workingFolder, version)) && IsRuntimeBinariesPresent()) return;

            if (Directory.Exists(workingFolder)) Directory.Delete(workingFolder, true);

            Directory.CreateDirectory(workingFolder);

            try
            {
                ZipFile.ExtractToDirectory(asset, workingFolder);
            }
            catch (Exception ex)
            {
                throw new InitializeException($"Failed extracting '{asset}' to '{workingFolder}'.", ex);
            }

            if (!IsRuntimeBinariesPresent())
                throw new InitializeException($"Asset '{asset}' was extracted, but no supported weasyprint executable was found in '{workingFolder}'.");

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
                    .WithWorkingDirectory(workingFolder)
                    .ExecuteAsync();

                if (command.ExitCode != 0 || stdErrBuffer.Length > 0) throw new InitializeException(command, stdErrBuffer.ToString());
            }
        }
        finally
        {
            initializeLock.Release();
        }
    }

    /// <summary>
    ///     Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <param name="html">html content to be converted to pdf</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintResult> Print(string html, params string[] additionalParameters)
    {
        return await Print(html, default, additionalParameters);
    }

    /// <summary>
    ///     Prints the given html to pdf using the weasyprint library.
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
    ///     Prints the given html to pdf using the weasyprint library.
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
            .WithArguments(arguments =>
            {
                arguments.Add("-");
                arguments.Add("-");
                arguments.Add("--encoding");
                arguments.Add("utf8");
                arguments.Add("--base-url");
                arguments.Add(baseUrl);
                AddAdditionalParameters(arguments, additionalParameters);
            })
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .WithStandardInputPipe(PipeSource.FromString(html, Encoding.UTF8))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);

        outputStream.Seek(0, SeekOrigin.Begin);

        IgnoreCertainErrors(stdErrBuffer);

        return new PrintStreamResult(
            outputStream,
            stdErrBuffer.ToString(),
            result.RunTime,
            result.ExitCode
        );
    }

    /// <summary>
    ///     Prints the given html to pdf using the weasyprint library.
    /// </summary>
    /// <param name="htmlFile">html file name with path to be converted to pdf</param>
    /// <param name="pdfFile">pdf file name with path for output</param>
    /// <param name="cancellationToken">Optional cancellationToken, passed to the executing command</param>
    /// <param name="additionalParameters">list of additional parameter for weasyprint (see readme.md#Weasyprint-CLI)</param>
    public async Task<PrintResult> Print(string htmlFile, string pdfFile, CancellationToken cancellationToken = default, params string[] additionalParameters)
    {
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
            .WithArguments(arguments =>
            {
                arguments.Add("--encoding");
                arguments.Add("utf8");
                arguments.Add("--base-url");
                arguments.Add(baseUrl);
                AddAdditionalParameters(arguments, additionalParameters);
                arguments.Add(htmlFile);
                arguments.Add(pdfFile);
            })
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);

        IgnoreCertainErrors(stdErrBuffer);

        return new PrintResult(
            Array.Empty<byte>(),
            stdErrBuffer.ToString(),
            result.RunTime,
            result.ExitCode
        );
    }

    private static void IgnoreCertainErrors(StringBuilder stdErrBuffer)
    {
        // Remove all lines containing 'test' from stdErrBuffer
        var filteredStdErr = stdErrBuffer.ToString().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => line.IndexOf("GLib-GIO-WARNING", StringComparison.OrdinalIgnoreCase) < 0)
            .ToArray();
        stdErrBuffer.Clear();
        stdErrBuffer.Append(string.Join(Environment.NewLine, filteredStdErr));
    }

    private Command BuildOsSpecificCommand()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            throw new PlatformNotSupportedException("Only Windows and Linux are supported by Weasyprint.Wrapped.");

        var (executablePath, executableFolder) = GetRuntimeExecutablePath();
        return Cli
            .Wrap(executablePath)
            .WithWorkingDirectory(executableFolder);
    }

    public async Task<VersionResult> Version()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var result = await BuildOsSpecificCommand()
            .WithArguments(arguments => arguments.Add("--info"))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8);

        IgnoreCertainErrors(stdErrBuffer);

        return new VersionResult(
            stdOutBuffer.ToString(),
            stdErrBuffer.ToString(),
            result.RunTime,
            result.ExitCode
        );
    }

    private static void AddAdditionalParameters(CliWrap.Builders.ArgumentsBuilder arguments, IEnumerable<string> additionalParameters)
    {
        foreach (var parameter in additionalParameters.Where(p => !string.IsNullOrWhiteSpace(p)))
            arguments.Add(parameter, false);
    }

    private bool IsRuntimeBinariesPresent()
    {
        try
        {
            _ = GetRuntimeExecutablePath();
            return true;
        }
        catch (InitializeException)
        {
            return false;
        }
    }

    private (string executablePath, string executableFolder) GetRuntimeExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var nestedExecutable = Path.Combine(workingFolder, "weasyprint-windows", "weasyprint.exe");
            if (File.Exists(nestedExecutable)) return (nestedExecutable, Path.GetDirectoryName(nestedExecutable)!);

            var flatExecutable = Path.Combine(workingFolder, "weasyprint.exe");
            if (File.Exists(flatExecutable)) return (flatExecutable, workingFolder);

            throw new InitializeException($"Unable to find weasyprint executable in '{workingFolder}'. Expected '{nestedExecutable}' or '{flatExecutable}'.");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var nestedExecutable = Path.Combine(workingFolder, "weasyprint-linux", "weasyprint");
            if (File.Exists(nestedExecutable)) return (nestedExecutable, Path.GetDirectoryName(nestedExecutable)!);

            var flatExecutable = Path.Combine(workingFolder, "weasyprint");
            if (File.Exists(flatExecutable)) return (flatExecutable, workingFolder);

            throw new InitializeException($"Unable to find weasyprint executable in '{workingFolder}'. Expected '{nestedExecutable}' or '{flatExecutable}'.");
        }

        throw new PlatformNotSupportedException("Only Windows and Linux are supported by Weasyprint.Wrapped.");
    }
}