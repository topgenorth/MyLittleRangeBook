using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class UnexpectedFitFileTypeError : Error
    {
        public UnexpectedFitFileTypeError(int expectedFileType) : base(
            $"Can only handle FIT file type {expectedFileType}")
        {
            Metadata.Add("ExpectedFileTypeId", XeroShotSessionParser.ExpectedFileType);
        }
    }
}
