using System.ComponentModel.DataAnnotations;
using System.IO;

namespace net.opgenorth.xero.data.sqlite
{
    public class SqliteOptions
    {
        /// <summary>
        ///     The name of the .sqlite database file (excluding the directory).
        /// </summary>
        public const string DefaultFileName = "garmin-shotview.sqlite";

        /// <summary>
        ///     The configuration key to get the directory for the Sqlite database.
        /// </summary>
        // Sqlite:DataDirectory
        public const string ConfigSection = "Sqlite";

        // readonly string _defaultDataDirectory = Path.Combine(
        //     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        //     ".mlrb");

        string _dataDirectory = string.Empty;

        /// <summary>
        ///     The directory holding the .sqlite file.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        public string DataDirectory
        {
            get => _dataDirectory;
            internal set => _dataDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public string SqliteFile => Path.Combine(_dataDirectory, DefaultFileName);

        public override string ToString() => SqliteFile;
        public override int GetHashCode() => SqliteFile.GetHashCode();
    }
}
