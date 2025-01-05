using System;
using System.IO;

namespace net.opgenorth.xero.data.sqlite
{
    public class SqliteOptions
    {
        readonly string _defaultDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".mlrb");

        string _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".mlrb");

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
        public string DataDirectory
        {
            get { return _dataDirectory; }
            internal set
            {
                _dataDirectory = string.IsNullOrWhiteSpace(value) ? _defaultDataDirectory : value;
            }
        }

        public string SqliteFile
        {
            get
            {
                return Path.Combine(_dataDirectory, DefaultFileName);
            }
        }

        public override string ToString() => SqliteFile;
        public override int GetHashCode() => SqliteFile.GetHashCode();
    }
}
