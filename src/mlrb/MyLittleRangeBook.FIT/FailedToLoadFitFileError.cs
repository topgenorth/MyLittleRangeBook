using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class FailedToLoadFitFileError : Error
    {
        public FailedToLoadFitFileError(string file) : base($"Could not load the FIT file {file}.")
        {
        }
    }
}
