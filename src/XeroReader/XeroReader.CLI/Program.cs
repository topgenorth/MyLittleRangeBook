using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using net.opgenorth.xero;
using net.opgenorth.xero.Commands;
using net.opgenorth.xero.Commands.ShotViewExcelWorkbook;
using net.opgenorth.xero.data.sqlite;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile($"appsettings.{Environments.Development}.json", true);
builder.AddGarminShotViewDatabase();
builder.Services.AddSerilog(lc =>
    lc .ReadFrom.Configuration(builder.Configuration)
);

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

var sp = scope.ServiceProvider;
var o = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<GarminShotViewSqliteOptions>>();

ILogger log = scope.ServiceProvider.GetRequiredService<ILogger>();
ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

app.Add<WorkbookCLI>("worksheet");
app.Add<SqliteMigrations>("database");
log.Verbose("Running app");
await app.RunAsync(args);
