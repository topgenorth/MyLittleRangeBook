namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Thrown when attempting to insert a FIT file with a name that already exists in the database - this indicates that
    ///     the FIT file was already added.
    /// </summary>
    public class DuplicateFitFileNameError : MlrbBaseError
    {
        public DuplicateFitFileNameError(string fileName) : base($"The file {fileName} already exists in the FIT table")
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
