using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class UnsupportedFitFileTypeError : Error
    {
        public UnsupportedFitFileTypeError(int expectedFileType) : base(
            $"Can only handle FIT file type {expectedFileType}")
        {
            Metadata.Add("ExpectedFileTypeId", XeroShotSessionParser.EXPECTED_FILE_TYPE);
        }
    }
}
