namespace MyLittleRangeBook.CLI.Database
{
    public class FailedToWriteFitFileToDatabaseError : MlrbBaseError
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="file">The name of the FIT file.</param>
        /// <param name="size">Size of the FIT file.</param>
        public FailedToWriteFitFileToDatabaseError(string file, int size) : base(
            $"Could not write {size} bytes from FIT file '{file}' to database.",
            ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE)
        {
            Metadata.Add("Filename", file);
            Metadata.Add("Size", size);
        }
    }
}
