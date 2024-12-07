

using System.IO;
using DbUp;
using DbUp.Builder;
using Microsoft.Data.Sqlite;
using Serilog;

namespace MyLittleRangebook.Data.Sqlite
{
    public class DbZookeeper
    {
        static readonly string DefaultFileName = "garmin-shotview.sqlite";

        readonly FileInfo _sqliteFile;
        readonly ILogger _logger;

        public DbZookeeper(ILogger logger, string dataDirectory)
        {
            _logger = logger;

            string p = Path.Combine(dataDirectory, DefaultFileName);
            _sqliteFile = new FileInfo(p);

            SqliteConnectionStringBuilder b = new($"Data Source={_sqliteFile.FullName}");
            ConnectionString = b.ConnectionString;

            _logger.Verbose("Connection string {connectionString}", ConnectionString);
        }

        public string ConnectionString { get; }

        public string SqliteFile => _sqliteFile.FullName;

        /// <summary>
        /// Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        public void UpdateDatabase()
        {
            var deployer = DeployChanges.To
                .SQLiteDatabase(ConnectionString)
                .LogToConsole()
                .WithScriptsEmbeddedInAssembly(GetType().Assembly)
                .Build();

            deployer.PerformUpgrade();
        }
        /// <summary>
        /// Will delete the <b>.sqlite</b> file if it exists, create a new one, and apply migrations.
        /// </summary>
        public void CreateDatabase()
        {
            if (_sqliteFile.Exists)
            {
                _logger.Verbose("Deleting file at {FileToDelete}", _sqliteFile.FullName);
                _sqliteFile.Delete();
            }
            else
            {
                _logger.Verbose("No database exists at {FileToDelete}", _sqliteFile.FullName);
            }
            UpdateDatabase();
        }

        public override int GetHashCode() => _sqliteFile.GetHashCode();

        public override string ToString() => _sqliteFile.FullName;
    }
}
