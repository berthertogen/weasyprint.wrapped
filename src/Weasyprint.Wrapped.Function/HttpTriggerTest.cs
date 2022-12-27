using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Weasyprint.Wrapped
{
    public static class HttpTriggerTest
    {
        [FunctionName("HttpTriggerTest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            Printer printer = null;
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                printer = new Printer(new ConfigurationProvider("C:/Github/weasyprint.wrapped/assets", true, "weasyprinter", false));
            }
            else
            {
                var workingDir = Path.Combine(Path.GetTempPath(), "weasyprinter");
                printer = new Printer(new ConfigurationProvider(context.FunctionAppDirectory, true, workingDir, true));
            }
            log.LogInformation("Start initializing wrapper");
            await printer.Initialize();
            log.LogInformation("Done initializing wrapper");
            log.LogInformation("Version");
            var versionResult = await printer.Version();
            log.LogInformation($" - ExitCode:            {versionResult.ExitCode}");
            log.LogInformation($" - HasError:            {versionResult.HasError}");
            log.LogInformation($" - Error:               {versionResult.Error}");
            log.LogInformation($" - RunTime:             {versionResult.RunTime}");
            log.LogInformation($" - Version:             {versionResult.Version}");
            log.LogInformation("Start printing");
            var result = await printer.Print(@"
            <html>
            <body>
            <h1>Hello from Azure Function</h1>
            </body>
            </html>
            ");
            log.LogInformation("Done printing");
            log.LogInformation($" - ExitCode:            {result.ExitCode}");
            log.LogInformation($" - HasError:            {result.HasError}");
            log.LogInformation($" - Error:               {result.Error}");
            log.LogInformation($" - RunTime:             {result.RunTime}");
            log.LogInformation($" - Bytes(length):       {result.Bytes.Length}");

            return new FileContentResult(result.Bytes, "application/pdf") // change octet-stream to pdf
            {
                FileDownloadName = "result.pdf"
            };
        }
    }
}
