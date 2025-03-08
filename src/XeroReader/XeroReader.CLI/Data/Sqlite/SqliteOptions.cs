namespace net.opgenorth.xero.Data.Sqlite;

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

    private readonly string _defaultDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".mlrb");

    private string _dataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".mlrb");

    /// <summary>
    ///     The directory holding the .sqlite file.
    /// </summary>
    public string DataDirectory
    {
        get => _dataDirectory;
        internal set => _dataDirectory = string.IsNullOrWhiteSpace(value) ? _defaultDataDirectory : value;
    }

    public string SqliteFile => Path.Combine(_dataDirectory, DefaultFileName);

    public override string ToString() => SqliteFile;
    public override int GetHashCode() => SqliteFile.GetHashCode();
}
