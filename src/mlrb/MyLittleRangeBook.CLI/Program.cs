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

IAppSettingsBootstrapper appSettingsBootstrapper = new AppSettingsBootstrapper();
await appSettingsBootstrapper.EnsureAppSettingsExistsAsync();

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.Sources.Clear();

if (EnvironmentHelper.IsProduction)
{
    builder.Configuration
        .AddJsonFile(DefaultAppSettingsFile.FullName, false, true);
}
else
{
    builder.Configuration
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
        .AddJsonFile(DefaultAppSettingsFile.FullName, true, true);
    builder.Services.AddPostgresHelper(builder.Configuration);
}
builder.Configuration.AddEnvironmentVariables();

builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.TryAddSingleton<ICliDisplay, CliDisplay>();
builder.Services.TryAddSingleton<IXeroShotSessionParser, XeroShotSessionParser>();
builder.Services.AddMyLittleRangeBookSqlite(builder.Configuration)
    .AddSerilog(lc =>
    {
        lc.WriteTo.Console();
        lc.WriteTo.Debug();

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
