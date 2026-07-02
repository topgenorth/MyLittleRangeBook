using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.GUI.Properties
{
    /// <summary>
    ///     This class defines the App settings. It implements INotifyPropertyChanged so that the App can
    ///     subscribe to settings changes.
    /// </summary>
    public class MlrbAppSettings : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets the default settings instance.
        /// </summary>
        public static MlrbAppSettings Default { get; } = new();

        /// <summary>
        ///     Gets or sets the AppTheme where allowed values are: <br />
        ///     <see cref="ThemeVariant.Light" />, <see cref="ThemeVariant.Dark" /> and <see cref="ThemeVariant.Default" /> <br />
        ///     We use the Light-theme as the default value.
        /// </summary>
        /// <remarks>
        ///     ThemeVariant.Default will try to inherit the systems theme.
        ///     <seealso href="https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants" />
        /// </remarks>
        public string AppTheme
        {
            get;
            set => SetField(ref field, value);
        } = ThemeVariant.Light.ToString();


        public string? LogsDirectory
        {
            get;
            set => SetField(ref field, value);
        }

        /// <summary>
        ///     Gets or sets the accent color to use. The default is Avalonia-blue.
        /// </summary>
        /// <remarks>
        ///     Since we want to reduce reflection usage as much as possible, we use a <see cref="JsonConverter" />
        ///     to manage the parsing.
        /// </remarks>
        [JsonConverter(typeof(JsonColorConverter))]
        public Color AccentColor
        {
            get;
            set => SetField(ref field, value);
        } = new(0xFF, 0x35, 0x78, 0xE5); // "#FF3578E5", Avalonia-Blue

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        // implementation for PropertyChanged
        // ReSharper disable once MemberCanBePrivate.Global
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // We save the settings automatically with each change. The alternative would be to provide a "save"-Button
        // in the Settings-UI.
        // implementation for PropertyChanged
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        internal Task LoadSettingsAsync()
        {
            IConfiguration config = App.Services.GetRequiredService<IConfiguration>();
            LogsDirectory = Path.GetDirectoryName(config["Serilog:WriteTo:2:Args:path"] ?? string.Empty) ?? "UNKNOWN";
            return Task.CompletedTask;
        }
    }
}