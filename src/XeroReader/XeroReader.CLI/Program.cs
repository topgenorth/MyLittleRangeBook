using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using net.opgenorth.xero;

// ILogger log = new LoggerConfiguration()
//     .MinimumLevel.Verbose()
//     .WriteTo.Console()
//     .WriteTo.Debug()
//     .CreateLogger();
// Log.Logger = log;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddSerilog(lc => lc
    .ReadFrom.Configuration(builder.Configuration));
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile($"appsettings.{Environments.Development}.json", true);


using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
var log = scope.ServiceProvider.GetRequiredService<ILogger>();

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

// app.Add<SimpleFitReader>();
// app.Add<ImportFitFile>();
// app.Add<ReadShotViewXLSX>();

app.Add<ReadShotViewXLSX>();
app.Add<SqliteMigrationCommmands>();
log.Verbose("Start app");
app.Run(args);
