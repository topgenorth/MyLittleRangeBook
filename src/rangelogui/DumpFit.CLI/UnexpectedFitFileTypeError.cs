using FluentResults;

namespace MySimpleRangeLog.CLI
{
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
