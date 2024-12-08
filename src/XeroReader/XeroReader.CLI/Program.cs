using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using net.opgenorth.xero;
using net.opgenorth.xero.ImportFitFiles;
using net.opgenorth.xero.shotview;
using Serilog.Core;

await using Logger log = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();
Log.Logger = log;

log.Verbose("Boostrapping app.");
ServiceCollection services = new();
services.AddSingleton<ILogger>(log);

using ServiceProvider serviceProvider = services.BuildServiceProvider();
ConsoleApp.ServiceProvider = serviceProvider;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

// app.Add<SimpleFitReader>();
// app.Add<ImportFitFile>();
// app.Add<ReadShotViewXLSX>();

app.Add<ReadShotViewXLSX>();
app.Add<SqliteMigrationCommmands>();
log.Verbose("Start app");
app.Run(args);
