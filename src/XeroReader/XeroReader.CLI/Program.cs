
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using net.opgenorth.xero;

await using var log = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();
Log.Logger = log;

log.Verbose("Boostrapping app.");
var services = new ServiceCollection();
services.AddSingleton<ILogger>(log);

using var serviceProvider = services.BuildServiceProvider();
ConsoleApp.ServiceProvider = serviceProvider;

var app = ConsoleApp.Create();

app.Add<SimpleFitReader>("");

log.Verbose("Start app");
app.Run(args);
