using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.PgSQL;
using Spectre.Console;
using static MyLittleRangeBook.Config.ConfigurationExtensions;

IAppSettingsBootstrapper bootstrapper = new AppSettingsJsonFileBootstrapper()
    .AddBootStrapper(AppSettingsJsonFileBootstrapper.DefaultBootStrappers)
    .AddBootStrapper(SqliteHelperExtensions.SqliteConnectionStringBootStrapper);
await bootstrapper.EnsureAppSettingsExistsAsync(DefaultAppSettingsFile.FullName);

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.AddMyLittleRangeBookJsonFiles();

builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.TryAddSingleton<ICliDisplay, CliDisplay>();

builder.Services.TryAddSingleton<IXeroShotSessionParser, XeroShotSessionParser>();

builder.Services.AddMyLittleRangeBookSqlite(builder.Configuration)
    .AddSerilog(lc =>
    {

        // TODO [TO20260501] Move all of this into the appsettings.json.
        lc.WriteTo.Debug()
            .WriteTo.MlrbLogFiles();

        if (builder.Environment.IsProduction())
        {
            lc.MinimumLevel.Warning();
        }
        else if (builder.Environment.IsStaging())
        {
            lc.MinimumLevel.Information();
            lc.WriteTo.Console();
        }
        else
        {
            lc.MinimumLevel.Verbose();
            lc.WriteTo.Console();
        }
    });

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

await app.RunAsync(args);

await Log.CloseAndFlushAsync();
