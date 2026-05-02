using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using Spectre.Console;
using static MyLittleRangeBook.Config.ConfigurationExtensions;

IAppSettingsBootstrapper bootstrapper = new AppSettingsJsonFileBootstrapper()
    .AddBootStrapper(SerilogAppSettingsJsonFileBootstrap.SerilogSection)
    .AddBootStrapper(SqliteHelperExtensions.SqliteConnectionStringBootStrapper);
await bootstrapper.EnsureAppSettingsExistsAsync(DefaultAppSettingsFile.FullName);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddMyLittleRangeBookConfig();
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.TryAddSingleton<ICliDisplay, CliDisplay>();
builder.Services.AddMyLittleRangeBookSqlite(builder.Configuration);
builder.Services.TryAddSingleton<IXeroShotSessionParser, XeroShotSessionParser>();

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

await app.RunAsync(args);

await Log.CloseAndFlushAsync();
