using FluentResults;

namespace MyLittleRangeBook.CLI
{
    /// <summary>
    /// Base class for FluentResult errors that will return a specific return code for errors within the app.
    /// </summary>
    public abstract class MyLittleRangeBookCliError: Error
    {
        protected MyLittleRangeBookCliError(string message, int returnCode = ReturnCodes.FAILURE) : base(message)
        {
            ReturnCode = returnCode;
            Metadata.Add("ReturnCode", ReturnCode);
        }

        public int ReturnCode { get; }
    }
}
