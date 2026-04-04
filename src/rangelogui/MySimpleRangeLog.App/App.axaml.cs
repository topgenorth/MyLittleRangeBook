using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Database.Sqlite;
using MySimpleRangeLog.Database;
using MySimpleRangeLog.Main.ViewModels;
using MySimpleRangeLog.Properties;
using MySimpleRangeLog.Services;
using MainView = MySimpleRangeLog.Main.Views.MainView;
using MainWindow = MySimpleRangeLog.Main.Views.MainWindow;

namespace MySimpleRangeLog
{
    /// <summary>
    ///     Main application entry point for My Simple Range App app.
    ///     Handles service registration, settings management, theme updates, and UI initialization.
    /// </summary>
    /// <remarks>
    ///     Key responsibilities:
    ///     - Registers all application services via DI
    ///     - Manages settings persistence and live UI updates (e.g., accent color changes)
    ///     - Handles platform-specific UI initialization (desktop vs single-view)
    ///     - Configures data validation strategy (disables Avalonia's built-in validation to avoid duplicates)
    ///     Service registration pattern:
    ///     - Services are registered only if not already registered (prevents duplicates)
    ///     - Settings.Default is loaded after DI container is built
    ///     - Design mode uses a separate service collection to avoid runtime dependencies
    ///     Theme management:
    ///     - Accent color from settings is applied to all FluentTheme palettes
    ///     - Changes are reactive — updating settings updates the UI immediately
    ///     Data validation:
    ///     - Both Avalonia and CommunityToolkit.Mvvm have DataAnnotation validators
    ///     - This app uses CommunityToolkit's [NotifyDataErrorInfo], so we disable Avalonia's to avoid duplicate errors
    /// </remarks>
    public class App : Application
    {
        public static IServiceProvider Services { get; set; } = null!;


        public static async void RegisterAppServices(IServiceCollection services)
        {
            try
            {
                services.AddSqliteHelper();
                services.TryAddSingleton<IDatabaseService, DesignDbService>();
                services.TryAddSingleton<ISimpleRangeEventService, SimpleRangeEventService>();
                services.TryAddSingleton<IFirearmsService, FirearmsService>();

                Services = services.BuildServiceProvider();

                await Settings.Default.LoadSettingsAsync();
            }
            catch (Exception ex)
            {
                // Log to standard trace listeners (Debug window in IDE, or debugView.exe)
                Trace.TraceError(ex.ToString());
            }
        }

        public override void Initialize()
        {
            // If we run in design mode (VS Designer, Blend), register design-time services
            if (Design.IsDesignMode)
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.TryAddSingleton<IDatabaseService>(new DesignDbService());
                serviceCollection.TryAddSingleton<ISettingsStorageService>(new JsonSettingsFileStorageService());
                RegisterAppServices(serviceCollection);
            }

            // Subscribe to settings changes so UI updates immediately when accent color changes
            Settings.Default.PropertyChanged += SettingsOnPropertyChanged;

            AvaloniaXamlLoader.Load(this);

            UpdateAccentColor(Settings.Default.AccentColor);
        }

        void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Default.AccentColor):
                    UpdateAccentColor((sender as Settings)?.AccentColor);

                    break;
            }
        }

        void UpdateAccentColor(Color? accentColor)
        {
            var fluentTheme = Styles.OfType<FluentTheme>().FirstOrDefault();
            if (fluentTheme is null || accentColor is null)
            {
                return;
            }

            foreach (var palette in fluentTheme.Palettes.Values)
            {
                palette.Accent = accentColor.Value;
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
            }

            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        ///     Removes Avalonia's built-in DataAnnotations validation plugin to prevent duplicate validation errors.
        /// </summary>
        /// <remarks>
        ///     Why suppress trim analysis?
        ///     - The BindingPlugins collection is accessed via reflection at runtime
        ///     - Trim analysis can't prove these types are preserved
        ///     - We know this is safe because we always use CommunityToolkit validation
        ///     Why remove these plugins?
        ///     - CommunityToolkit.Mvvm provides [NotifyDataErrorInfo] and [ValidateProperty]
        ///     - Avalonia's DataAnnotationsValidationPlugin does the same thing
        ///     - Having both would show the same error twice (bad UX)
        ///     What's preserved?
        ///     - The [NotifyDataErrorInfo] plugin from CommunityToolkit remains active
        ///     - All validation logic is still fully functional
        /// </remarks>
        [UnconditionalSuppressMessage("Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "This is well tested and known to work as intended.")]
        void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
