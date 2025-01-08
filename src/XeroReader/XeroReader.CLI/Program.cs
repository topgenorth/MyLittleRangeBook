using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.Commands;
using net.opgenorth.xero.Commands.ShotViewExcelWorkbook;
using net.opgenorth.xero.data.sqlite;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
#if DEBUG
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile($"appsettings.{Environments.Development}.json", true);
#else
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true);
#endif


string appSettingsJson = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
if (!File.Exists(appSettingsJson))
{
    builder.Services.AddSerilog(lc =>
    {
        lc.WriteTo.Console()
            .Enrich.WithProperty("ApplicationName", "xeror")
            .MinimumLevel.Verbose();
    });
}
else
{
    builder.Services.AddSerilog(lc =>
        lc.ReadFrom.Configuration(builder.Configuration)
    );
}

builder.AddSqliteDatabase();
builder.Services.AddWorksheetSqlite();

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

IServiceProvider? sp = scope.ServiceProvider;
IOptionsSnapshot<SqliteOptions>? o = scope.ServiceProvider
    .GetRequiredService<IOptionsSnapshot<SqliteOptions>>();

ILogger log = scope.ServiceProvider.GetRequiredService<ILogger>();
ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

// [TO20241226] Add the CLI.
app.Add<WorkbookCLI>("workbook");
app.Add<SqliteMigrations>("database");
log.Verbose("Running app");
await app.RunAsync(args);
