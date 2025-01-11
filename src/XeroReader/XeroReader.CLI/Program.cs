using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using net.opgenorth.xero.Commands;
using net.opgenorth.xero.Commands.ShotViewExcelWorkbook;
using net.opgenorth.xero.data.sqlite;
using net.opgenorth.xero.shotview;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.json", true, true);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);
#endif

OptionsBuilder<SqliteOptions> optionsBuilder = builder.Services.AddOptions<SqliteOptions>();
#pragma warning disable IL2026
optionsBuilder.ValidateDataAnnotations();
#pragma warning restore IL2026
optionsBuilder.ValidateOnStart();
optionsBuilder.BindConfiguration(SqliteOptions.ConfigSection);

builder.Services.AddSerilog(lc =>
{
    lc.ReadFrom.Configuration(builder.Configuration);
    lc.MinimumLevel.Verbose();
});


builder.Services.TryAddScoped<IDbZookeeper, SqliteDbZookeeper>();
builder.Services.TryAddScoped<IGetShotSession, MyLittleRangeBookRepository>();
builder.Services.TryAddScoped<IPersistShotSession, MyLittleRangeBookRepository>();
builder.Services.TryAddScoped<MyLittleRangeBookRepository>();


using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ILogger log = scope.ServiceProvider.GetRequiredService<ILogger>();
SqliteOptions? options = builder.Configuration.GetSection(SqliteOptions.ConfigSection).Get<SqliteOptions>();

if (options is null)
{
    log.Warning("Missing SQLite settings");
}
else
{
    log.Information("Database {db}.", options.SqliteFile);
}


ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
// [TO20241226] Add the CLI.
app.Add<WorkbookCLI>("workbook");
app.Add<SqliteMigrations>("database");
log.Verbose("Running app");
await app.RunAsync(args);
