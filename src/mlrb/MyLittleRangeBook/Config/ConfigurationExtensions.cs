namespace MyLittleRangeBook.Config
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     The name of the default database.
        /// </summary>
        internal static readonly string SqliteDatabaseName = "mlrb.db";
        internal static readonly string AppSettingsFileName = "appsettings.json";

        internal static readonly string DefaultLocalAppDataFolder =
            OperatingSystem.IsWindows() ? "MyLittleRangeBook" : ".mylittlerangebook";

        /// <summary>
        ///     Gets the user settings directory path for this application.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        internal static DirectoryInfo DefaultUserSettingsDirectory => new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            DefaultLocalAppDataFolder));

        public static FileInfo DefaultAppSettingsFile =>
            new FileInfo(Path.Combine(DefaultUserSettingsDirectory.FullName, AppSettingsFileName))
                .InjectEnvironmentIntoFileName();

        /// <summary>
        ///     Determines the full file path for the SQLite database based on the current environment.
        ///     Suffixes the database name with the environment name (e.g., Development) if not in Production.
        /// </summary>
        /// <param name="inferFromEnvironment">
        ///     If set to true, then the database name will be suffixed with the current environment
        ///     name (e.g., Development). Defaults to true.
        /// </param>
        /// <returns>The full path to the SQLite database file.</returns>
        public static string DefaultSqliteDatabaseName(bool inferFromEnvironment = true)
        {
            string fullPath = Path.Combine(DefaultUserSettingsDirectory.FullName, SqliteDatabaseName);
            if (inferFromEnvironment)
            {
                fullPath = new FileInfo(fullPath).InjectEnvironmentIntoFileName().FullName;
            }

            if (!OperatingSystem.IsWindows())
            {
                fullPath = fullPath.ToLowerInvariant();
            }

            return fullPath;
        }
    }
}
