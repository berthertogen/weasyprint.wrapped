using Weasyprint.Wrapped;

var printer = new Printer(new ConfigurationProvider());
Console.WriteLine("Start initializing wrapper");
printer.Initialize();
Console.WriteLine("Done initializing wrapper");
Console.WriteLine("Start printing");
var result = await printer.Print("<html><body><h1>TEST</h1></body></html>");
Console.WriteLine("Done printing");
Console.WriteLine($" - ExitCode:            {result.ExitCode}");
Console.WriteLine($" - HasError:            {result.HasError}");
Console.WriteLine($" - Error:               {result.Error}");
Console.WriteLine($" - Output:              {result.Output}");
Console.WriteLine($" - TempOutputFile:      {result.TempOutputFile}");
Console.WriteLine($" - RunTime:             {result.RunTime}");
Console.WriteLine($" - Bytes(length):       {result.Bytes.Length}");
