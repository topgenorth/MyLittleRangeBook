using System.IO;

namespace MyLittleRangebook.Data.Sqlite
{
    public class GarminShotViewSqliteOptions
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

        /// <summary>
        ///     The directory holding the .sqlite file.
        /// </summary>
        public string DataDirectory { get; set; }

        public string SqliteFile => Path.Combine(string.IsNullOrEmpty(DataDirectory) ? string.Empty : DataDirectory,
            DefaultFileName);

        public override string ToString() => SqliteFile;
        public override int GetHashCode() => SqliteFile.GetHashCode();
    }
}
