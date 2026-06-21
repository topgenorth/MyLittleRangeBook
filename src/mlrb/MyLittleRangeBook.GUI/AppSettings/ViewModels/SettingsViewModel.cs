using System.IO;
using Avalonia.Styling;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.GUI.Properties;
using MyLittleRangeBook.Persistence.Sqlite;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    /// <summary>
    ///     ViewModel responsible for managing application settings and data operations.
    ///     Handles theme switching, data import/export, and database management.
    /// </summary>
    public class SettingsViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IConfiguration _config;
        readonly ISqliteHelper  _sqliteHelper;

        public SettingsViewModel(ISqliteHelper sqliteHelper, IConfiguration config)
        {
            _sqliteHelper = sqliteHelper;
            _config       = config;
        }

        /// <summary>
        ///     Gets the application settings instance for binding to UI controls.
        /// </summary>
        public MlrbAppSettings MlrbAppSettings => MlrbAppSettings.Default;

        public string PathToDatabase
        {
            get
            {
                string                        s = _config.GetConnectionString("SqliteConnection") ?? "";
                SqliteConnectionStringBuilder b = new(s);
                return b.DataSource;
            }
        }

        public string PathToAssets
        {
            get
            {
                string dbDir    = Path.GetDirectoryName(PathToDatabase) ?? string.Empty;
                string assetDir = Path.Combine(dbDir, OperatingSystem.IsWindows() ? "Assets" : "assets");
                return assetDir;
            }
        }

        /// <summary>
        ///     Array of available theme variants that users can select from.
        ///     Includes Default, Dark, and Light theme options.
        /// </summary>
        [UsedImplicitly]
        public string[] AvailableThemeVariants { get; } =
            [
                ThemeVariant.Default.ToString(),
                ThemeVariant.Dark.ToString(),
                ThemeVariant.Light.ToString(),
            ];
    }
}