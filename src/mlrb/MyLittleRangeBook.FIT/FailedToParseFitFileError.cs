using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class FailedToParseFitFileError : Error
    {
        public FailedToParseFitFileError() : base("Could not parse the FIT file.")
        {
        }
    }
}