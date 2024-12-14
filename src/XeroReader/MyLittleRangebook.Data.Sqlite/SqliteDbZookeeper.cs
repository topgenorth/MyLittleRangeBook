using System.IO;
using DbUp;
using DbUp.Engine;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Serilog;

namespace MyLittleRangebook.Data.Sqlite
{
    public class SqliteDbZookeeper : IDbZookeeper
    {
        readonly ILogger _logger;
        readonly FileInfo _sqliteFile;

        public SqliteDbZookeeper(ILogger logger, IOptionsSnapshot<GarminShotViewSqliteOptions> options)
        {
            _logger = logger;
            _sqliteFile = new FileInfo(options.Value.SqliteFile);
            SqliteConnectionStringBuilder b = new($"Data Source={options.Value.SqliteFile}");
            ConnectionString = b.ConnectionString;
            _logger.Verbose("Connection string {connectionString}", ConnectionString);
        }

        public string ConnectionString { get; }

        public string SqliteFile => _sqliteFile.FullName;

        /// <summary>
        ///     Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        public void UpdateDatabase()
        {
            UpgradeEngine deployer = DeployChanges.To
                .SQLiteDatabase(ConnectionString)
                .LogToConsole()
                .WithScriptsEmbeddedInAssembly(GetType().Assembly)
                .Build();

            deployer.PerformUpgrade();
        }

        /// <summary>
        ///     Will delete the <b>.sqlite</b> file if it exists, create a new one, and apply migrations.
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
