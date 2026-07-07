using System.Text;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvents;
using Serilog.Exceptions;
using static MyLittleRangeBook.Config.ConfigurationExtensions;

// [TO20260510] For Spectre.Console - force UTF8
if (Console.OutputEncoding.CodePage == 437) // DOS/OEM encoding
{
    Console.OutputEncoding = Encoding.UTF8;
}

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
                                   .Enrich.WithEnvironmentName()
                                   .Enrich.WithExceptionDetails()
                                   .Enrich.FromLogContext();
                            });

#region Spectre.Console dependencies
builder.Services.TryAddSingleton(AnsiConsole.Console);
builder.Services.AddTransient<ICliDisplay, CliDisplay>();
builder.Services.AddTransient<SimpleAppHeader>();
builder.Services.AddTransient<ICommandHeaderPrinter, SimpleAppHeaderWithLogging>();
builder.Services.AddTransient<ISimpleRangeEventPrinter, SimpleRangeEventPrinter>();
builder.Services.AddTransient<ISimpleRangeEventListPrinter, SimpleRangeEventListPrinter>();
#endregion

builder.Services.AddTransient<IXeroShotSessionParser, XeroShotSessionParser>();
builder.Services.AddTransient<IXeroCsvShotSessionParser, XeroCsvShotSessionParser>();
builder.Services.RegisterMyLittleRangeBookSqlite(builder.Configuration)
       .RegisterEventSourcingStuff()
       .RegisterRangeEventStuff()
       .RegisterCartridges()
       .RegisterRangeAssetHandlers()
       .RegisterDomainEventSerializers()
       .RegisterRangeAssetEventSourcing()
       .RegisterFirearmEventSourcing()
    ;

using IHost         host  = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

ILogger logger = host.Services.GetRequiredService<ILogger>();
logger.Debug("MyLittleRangeBook CLI v{AppVersion}", typeof(ReturnCodes).Assembly.GetAssemblyVersionInformation());
#if DEBUG
logger.Debug("Serilog configured.");
#endif

await app.RunAsync(args).ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);