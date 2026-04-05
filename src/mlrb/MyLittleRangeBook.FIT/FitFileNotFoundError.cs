using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class FitFileNotFoundError : Error
    {
        public FitFileNotFoundError(string file) : base($"Could not find FIT file '{file}'")
        {
            Metadata.Add("Filename", file);
        }
    }
}