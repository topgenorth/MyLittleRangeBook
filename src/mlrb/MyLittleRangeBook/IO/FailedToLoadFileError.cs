using FluentResults;

namespace MyLittleRangeBook.IO
{
    public class FailedToLoadFileError : Error
    {
        public FailedToLoadFileError(string file) : base($"Could not read the contents of  `{file}`.")
        {
            Metadata.Add("file", file);
        }
    }
}
