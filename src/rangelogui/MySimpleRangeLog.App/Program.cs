using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Database.Sqlite;
using MySimpleRangeLog.Database;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog
{
    sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task Main(string[] args)
        {
            ConfigureLogging();

            // Register the Desktop service
            var services = new ServiceCollection();
            services.AddSqliteHelper();
            services.AddSingleton<ISettingsStorageService>(new JsonSettingsFileStorageService());
            var dbService = new SQLiteDbService();
            services.AddSingleton<IDatabaseService>(dbService);
            // [TO20260311] Need to register a handler to convert strings to DateTimeOffset values.
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new SQLiteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset?), new SQLiteDateTimeOffsetHandler());

            App.RegisterAppServices(services);

            try
            {
                Log.Information("Starting Avalonia app (AOT={AOT}), environment {Env}",
                    !RuntimeFeature.IsDynamicCodeCompiled,
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                );
                
                
                Log.Information(dbService.GetDatabaseName());
                Log.Information(dbService.GetConnectionString());
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
                // Create a dedicated 'logs' subdirectory for app logs
                var logDir = Path.Combine(JsonSettingsFileStorageService.SettingsDirectory, "logs");
                Directory.CreateDirectory(logDir);
                // Define log filename pattern (Serilog will append date for rolling)
                var logPath = Path.Combine(logDir, "app-.log");

                // Build Serilog configuration
                var cfg = new LoggerConfiguration();
                if (EnvironmentHelper.IsProduction)
                {
                    cfg.MinimumLevel.Information();
                }
                else if (EnvironmentHelper.IsStaging)
                {
                    cfg.MinimumLevel.Debug();
                }
                else
                {
                    cfg.MinimumLevel.Verbose();
                }

                // Write logs to files with daily rotation
                cfg.WriteTo.File(
                        logPath,
                        rollingInterval: RollingInterval.Day, // Create new log file each day
                        retainedFileCountLimit: 7, // Keep only 7 days of logs
                        shared: true, // Allow multiple instances to write
                        flushToDiskInterval: TimeSpan.FromSeconds(1), // Periodically flush to disk
                        buffered: false, // Write directly for reliability
                        outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    ;

                // Install the configured logger as the global Serilog logger
                Log.Logger = cfg.CreateLogger();
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
