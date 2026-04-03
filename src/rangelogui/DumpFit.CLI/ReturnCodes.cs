namespace MySimpleRangeLog.CLI
{
    public static class ReturnCodes
    {
        public const int SUCCESS = 0;
        public const int FILE_NOT_FOUND = 1;
        public const int FAILED_TO_LOAD = 2;
        public const int FAILED_TO_PARSE = 3;
        public const int FAILED_TO_APPLY_MIGRATIONS = 4;

    }
}
