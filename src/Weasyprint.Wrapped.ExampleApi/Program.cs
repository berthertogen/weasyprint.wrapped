using Weasyprint.Wrapped;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(new Printer(new Weasyprint.Wrapped.ConfigurationProvider()));

var app = builder.Build();

var printer = app.Services.GetRequiredService<Printer>();
await printer.Initialize();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();