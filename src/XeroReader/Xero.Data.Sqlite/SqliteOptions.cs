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


        string _dataDirectory = string.Empty;

        /// <summary>
        ///     The directory holding the .sqlite file.
        /// </summary>
        public string DataDirectory
        {
            get => _dataDirectory;
             set => _dataDirectory= value;
        }

        public string SqliteFile => Path.Combine(_dataDirectory, DefaultFileName);

        public override string ToString() => SqliteFile;
        public override int GetHashCode() => SqliteFile.GetHashCode();
    }
}
