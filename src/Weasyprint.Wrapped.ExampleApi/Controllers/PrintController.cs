using Microsoft.AspNetCore.Mvc;

namespace Weasyprint.Wrapped.ExampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PrintController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public PrintController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Get")]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var printer = new Printer(new ConfigurationProvider());
        _logger.LogInformation("Start initializing wrapper");
        await printer.Initialize();
        _logger.LogInformation("Done initializing wrapper");
        _logger.LogInformation("Version");
        var versionResult = await printer.Version();
        _logger.LogInformation($" - ExitCode:            {versionResult.ExitCode}");
        _logger.LogInformation($" - HasError:            {versionResult.HasError}");
        _logger.LogInformation($" - Error:               {versionResult.Error}");
        _logger.LogInformation($" - RunTime:             {versionResult.RunTime}");
        _logger.LogInformation($" - Version:             {versionResult.Version}");
        _logger.LogInformation("Start printing");
        var result = await printer.Print(@"
            <html>
            <body>
            <h1>Hello from Azure Function</h1>
            </body>
            </html>
            ");
        _logger.LogInformation("Done printing");
        _logger.LogInformation($" - ExitCode:            {result.ExitCode}");
        _logger.LogInformation($" - HasError:            {result.HasError}");
        _logger.LogInformation($" - Error:               {result.Error}");
        _logger.LogInformation($" - RunTime:             {result.RunTime}");
        _logger.LogInformation($" - Bytes(length):       {result.Bytes.Length}");

        return new FileContentResult(result.Bytes, "application/pdf") // change octet-stream to pdf
        {
            FileDownloadName = "result.pdf"
        };
    }
}
