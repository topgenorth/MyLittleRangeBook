using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook.CLI;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.IO;
using Spectre.Console;
using static MyLittleRangeBook.Config.ConfigurationExtensions;

IAppSettingsBootstrapper bootstrapper = new AppSettingsJsonFileBootstrapper()
    .AddBootStrapper(SerilogAppSettingsJsonFileBootstrap.SerilogSection)
    .AddBootStrapper(SqliteHelperExtensions.SqliteConnectionStringBootStrapper);
await bootstrapper.EnsureAppSettingsExistsAsync(DefaultAppSettingsFile.FullName).ConfigureAwait(false);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddMyLittleRangeBookConfig();
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

#region Spectre.Console dependencies
builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.AddTransient<ICliDisplay, CliDisplay>();
builder.Services.AddTransient<ISimpleRangeEventPrinter, SimpleRangeEventPrinter>();
builder.Services.AddTransient<ISimpleRangeEventListPrinter, SimpleRangeEventListPrinter>();
#endregion

builder.Services.AddTransient<IXeroShotSessionParser, XeroShotSessionParser>();

#region SQLite dependencies
builder.Services.AddMyLittleRangeBookSqlite(builder.Configuration);
builder.Services.AddKeyedTransient<ISimpleRangeEventHelper, SqliteSimpleRangeEventHelper>(SqliteHelperExtensions
    .DI_KEYS_SQLITE);
#endregion

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

ILogger logger = host.Services.GetRequiredService<ILogger>();

logger.Information("MyLittleRangeBook CLI v{AppVersion}", typeof(ReturnCodes).Assembly.GetAssemblyVersionInformation());
await app.RunAsync(args).ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
