using FluentResults;

namespace MyLittleRangeBook.CLI.Database
{
    public class WroteFitFileToDatabaseSuccess : Success
    {
        public WroteFitFileToDatabaseSuccess(string file, int size) : base(
            $"Wrote {size} bytes from FIT file '{file}' to database.")
        {
            Metadata.Add("Filename", file);
            Metadata.Add("Size", size);
            Metadata.Add("ReturnCode", ReturnCodes.SUCCESS);
        }

        public int ResultCode => ReturnCodes.SUCCESS;
    }
}
