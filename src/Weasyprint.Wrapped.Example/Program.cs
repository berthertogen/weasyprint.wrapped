using Weasyprint.Wrapped;

var configurationProvider = new ConfigurationProviderWindows("./working/wp");

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Start initializing wrapper");
new Initializer(configurationProvider).Do();
Console.WriteLine("Done initializing wrapper");
Console.WriteLine("Start printing");
// new Printer().Do("<html><body><h1>TEST</h1></body></html>");
var runner = new CliRunnerWindows(configurationProvider);
new Printer(runner).Do("<html><body><h1>TEST</h1></body></html>");
Console.WriteLine("Done printing");
