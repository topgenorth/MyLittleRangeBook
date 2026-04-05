using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class FitFileNotFoundError : Error
    {
        public FitFileNotFoundError(string file) : base($"Could not find FIT file '{file}'")
        {
            Metadata.Add("FilePath", file);
        }
    }
    public class UnexpectedFitFileTypeError : Error
    {
        public UnexpectedFitFileTypeError(int expectedFileType) : base($"Can only handle FIT file type {expectedFileType}")
        {
            Metadata.Add("ExpectedFileTypeId", XeroShotSessionParser.ExpectedFileType);
        }
    }

    public class FailedToParseFitFileError : Error
    {
        public FailedToParseFitFileError() : base($"Could not parse the FIT file.")
        {
        }
    }
}
