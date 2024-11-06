using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using net.opgenorth.xero;
using net.opgenorth.xero.ImportFitFiles;
using Serilog.Core;

await using Logger log = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();
Log.Logger = log;

log.Verbose("Boostrapping app.");
ServiceCollection services = new ServiceCollection();
services.AddSingleton<ILogger>(log);

using ServiceProvider serviceProvider = services.BuildServiceProvider();
ConsoleApp.ServiceProvider = serviceProvider;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

app.Add<SimpleFitReader>("");
app.Add<ImportFitFile>("import");

log.Verbose("Start app");
app.Run(args);
