using FluentResults;

namespace MyLittleRangeBook.CLI.Database
{
    public class FailedToWriteFitFileToDatabaseError : Error
    {
        public FailedToWriteFitFileToDatabaseError(string file, int size) : base(
            $"Could not write {size} bytes from FIT file '{file}' to database.")
        {
            Metadata.Add("Filename", file);
            Metadata.Add("Size", size);
        }

        public int ResultCode => ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE;
    }
}
