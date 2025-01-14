using System.IO;
using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Options;
using Serilog;

namespace net.opgenorth.xero.data.sqlite
{
    public class SqliteDbZookeeper : IDbZookeeper
    {
        readonly ILogger _logger;
        readonly FileInfo _sqliteFile;

        public SqliteDbZookeeper(ILogger logger, IOptionsSnapshot<SqliteOptions> options)
        {
            _logger = logger;
            SqliteOptions o = options.Value.InferDataDirectory();
            _sqliteFile = new FileInfo(o.SqliteFile);
            ConnectionString = o.MakeSqliteConnectionString();
            _logger.Verbose("Connection string '{connectionString}'", ConnectionString);
        }

        public string ConnectionString { get; }

        public string SqliteFile => _sqliteFile.FullName;

        /// <summary>
        ///     Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        public void UpdateDatabase()
        {
            UpgradeEngine deployer = DeployChanges.To
                .SqliteDatabase(ConnectionString)
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
                _logger.Warning("Deleting file at {FileToDelete}.", _sqliteFile.FullName);
                _sqliteFile.Delete();
            }
            else
            {
                _logger.Information("Creating database {FileToDelete}.", _sqliteFile.FullName);
            }

            if (!_sqliteFile.Directory!.Exists)
            {
                _sqliteFile.Directory.Create();
            }

            UpdateDatabase();
        }

        public override int GetHashCode() => _sqliteFile.GetHashCode();

        public override string ToString() => _sqliteFile.FullName;
    }
}
