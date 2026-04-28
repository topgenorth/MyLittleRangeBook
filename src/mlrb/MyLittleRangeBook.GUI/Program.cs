using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.GUI.ViewModels;
using MyLittleRangeBook.Services;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI
{
    [UsedImplicitly]
    sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task Main(string[] args)
        {
            // [TO20260425] This has to run first and will create a default appsettings.json file if one does not exist.
            IAppSettingsBootstrapper appSettingsBootstrapper = new AppSettingsBootstrapper();
            await appSettingsBootstrapper.EnsureAppSettingsExistsAsync();


            ConfigureLogging();

            string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var services = new ServiceCollection();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSerilog(lc =>
            {
                string logDir = Path.Combine(JsonSettingsFileStorageService.SettingsDirectory, "logs");
                Directory.CreateDirectory(logDir);
                // Define log filename pattern (Serilog will append date for rolling)
                string logPath = Path.Combine(logDir, "app-.log");

                if (EnvironmentHelper.IsProduction)
                {
                    lc.MinimumLevel.Information();
                }
                else if (EnvironmentHelper.IsStaging)
                {
                    lc.MinimumLevel.Debug();
                }
                else
                {
                    lc.MinimumLevel.Verbose();
                }

                lc.WriteTo.Debug();

                // Write logs to files with daily rotation
                lc.WriteTo.File(
                        logPath,
                        rollingInterval: RollingInterval.Day, // Create new log file each day
                        retainedFileCountLimit: 7, // Keep only 7 days of logs
                        shared: true, // Allow multiple instances to write
                        flushToDiskInterval: TimeSpan.FromSeconds(1), // Periodically flush to disk
                        buffered: false, // Write directly for reliability
                        outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    ;
            });
            services.AddSingleton<IConfiguration>(configuration);

            services.AddMyLittleRangeBookSqlite(configuration);
            services.TryAddTransient<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>();
            services.TryAddTransient<IFirearmsService, SqliteFirearmsService>();

            services.TryAddSingleton<ISettingsStorageService>(new JsonSettingsFileStorageService());

            // Register the DialogService factory for creating dialog services with specific participants
            services.AddSingleton<Func<IDialogParticipant, IDialogService>>(provider =>
                participant => new DialogService(participant));

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
                Log.Debug("Application is shutting down");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }

        static void ConfigureLogging()
        {
            try
            {
                // Route Avalonia's internal Trace output through Serilog for unified logs
                Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());

                // Add global exception handlers to ensure uncaught errors are logged
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
        }
    }
}
