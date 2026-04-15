using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.PgSQL;
using Spectre.Console;
using SQLitePCL;

// [TO20260411] Make sure that we have appsettings.json.
IAppSettingsBootstrapper appSettings = new AppSettingsBootstrapper();
await appSettings.EnsureAppSettingsExistsAsync();

raw.SetProvider(new SQLite3Provider_e_sqlite3());
Batteries.Init();

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.TryAddSingleton<ICliDisplay, CliDisplay>();
builder.Services.TryAddSingleton<IXeroShotSessionParser, XeroShotSessionParser>();
builder.Services.AddMyLittleRangeBookSqlite(builder.Configuration)
    .AddPostgresHelper(builder.Configuration)
    .AddSerilog(lc =>
    {
        lc.WriteTo.Console();

        if (builder.Environment.IsProduction())
        {
            lc.MinimumLevel.Warning();
        }
        else if (builder.Environment.IsStaging())
        {
            lc.MinimumLevel.Information();
        }
        else
        {
            lc.MinimumLevel.Verbose();
        }
    });


using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

await app.RunAsync(args);

await Log.CloseAndFlushAsync();
