using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

using MyLittleRangeBook.Config;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.GUI.ViewModels;
using MyLittleRangeBook.Persistence.Sqlite;
using SharedControls.Services;
using ConfigurationExtensions = MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook.GUI
{
    [UsedImplicitly]
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task Main(string[] args)
        {
            var bootstrapper = new AppSettingsJsonFileBootstrapper()
                .AddBootStrapper(SerilogAppSettingsJsonFileBootstrap.SerilogSection)
                .AddBootStrapper(SqliteHelperExtensions.SqliteConnectionStringBootStrapper);
            await bootstrapper
                .EnsureAppSettingsExistsAsync(ConfigurationExtensions.DefaultAppSettingsFile.FullName)
                .ConfigureAwait(false);

            ConfigurationExtensions.DefaultLogDirectory.Create();

            var services = new ServiceCollection();

            var configuration = services.AddMyLittleRangeBookConfig();
            services.AddSerilog((serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(serviceProvider)
                    .Enrich.WithEnvironmentName()
                    .Enrich.FromLogContext();
            });


            // Add global exception handlers to ensure uncaught errors are logged
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                    Log.Fatal(e.ExceptionObject as Exception, "[FATAL] Unhandled exception in AppDomain");

                TaskScheduler.UnobservedTaskException += (_, e) =>
                {
                    Log.Error(e.Exception, "[ERROR] Unobserved task exception");
                    e.SetObserved(); // Prevents finalizer from re-raising the exception
                };
            }
            catch
            {
                // Last resort: silently fail to avoid crashing the app if logging setup fails (e.g., under AOT)
            }

            services.RegisterMyLittleRangeBookSqlite(configuration);
            services.RegisterRangeEventStuff()
                .RegisterCartridges()
                .RegisterRangeAssetHandlers()
                .RegisterDomainEventSerializers()
                .RegisterRangeAssetEventSourcing()
                .RegisterFirearmEventSourcing();

            services.AddSingleton<Func<IDialogParticipant, IDialogService>>(provider =>
                participant => new DialogService(participant));


            // Route Avalonia's internal Trace output through Serilog for unified logs
            Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());

            services.AddTransient<MainViewModel>();
            services.AddTransient<ManageSimpleRangeEventsViewModel>();
            services.AddTransient<ManageFirearmsViewModel>();
            services.AddTransient<SettingsViewModel>();

            App.RegisterAppServices(services);


            try
            {
                Log.Information("Starting Avalonia app (AOT={AOT}), environment {Env}",
                    !RuntimeFeature.IsDynamicCodeCompiled,
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                );

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");

                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
                Log.Verbose("Application is shutting down");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        // ReSharper disable once MemberCanBePrivate.Global
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }
    }
}
