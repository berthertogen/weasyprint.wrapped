using Weasyprint.Wrapped;

var printer = new Printer(new ConfigurationProvider());
Console.WriteLine("Start initializing wrapper");
await printer.Initialize();
Console.WriteLine("Done initializing wrapper");
Console.WriteLine("Start printing");
var result = await printer.Print(File.ReadAllText("../Weasyprint.Wrapped.Tests/Expected/Print_RunsCommand_SpecialCharacters_Input.html"));
Console.WriteLine("Done printing");
Console.WriteLine($" - ExitCode:            {result.ExitCode}");
Console.WriteLine($" - HasError:            {result.HasError}");
Console.WriteLine($" - Error:               {result.Error}");
Console.WriteLine($" - RunTime:             {result.RunTime}");
Console.WriteLine($" - Bytes(length):       {result.Bytes.Length}");
File.WriteAllBytes("result.pdf", result.Bytes);
