using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.GUI.Properties;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.GUI.ViewModels;
using MainView = MyLittleRangeBook.GUI.Views.MainView;
using MainWindow = MyLittleRangeBook.GUI.Views.MainWindow;

namespace MyLittleRangeBook.GUI
{
    public class App : Application
    {
        public static IServiceProvider Services { get; set; } = null!;


        public static async void RegisterAppServices(IServiceCollection services)
        {
            try
            {
                Services = services.BuildServiceProvider();
                await MlrbAppSettings.Default.LoadSettingsAsync();
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
                serviceCollection.TryAddSingleton<ISettingsStorageService>(new AppSettingsFileStorageService());
                RegisterAppServices(serviceCollection);
            }

            // Subscribe to settings changes so UI updates immediately when accent color changes
            MlrbAppSettings.Default.PropertyChanged += SettingsOnPropertyChanged;

            AvaloniaXamlLoader.Load(this);

            UpdateAccentColor(MlrbAppSettings.Default.AccentColor);
        }

        private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MlrbAppSettings.Default.AccentColor):
                    UpdateAccentColor((sender as MlrbAppSettings)?.AccentColor);

                    break;
            }
        }

        private void UpdateAccentColor(Color? accentColor)
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
                desktop.MainWindow = new MainWindow { DataContext = Services.GetRequiredService<MainViewModel>() };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                    { DataContext = Services.GetRequiredService<MainViewModel>() };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}