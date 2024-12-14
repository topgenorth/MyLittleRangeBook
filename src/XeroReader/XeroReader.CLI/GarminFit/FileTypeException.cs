namespace net.opgenorth.xero.GarminFit
{
    public class FileTypeException : Exception
    {
        public FileTypeException()
        {
        }

        public FileTypeException(string message) : base(message)
        {
        }

        public FileTypeException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
