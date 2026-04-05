namespace MyLittleRangeBook.Cli
{
    public static class ReturnCodes
    {
        public const int SUCCESS = 0;
        public const int DATABASE_FILE_NOT_FOUND = 1;
        public const int FAILED_TO_LOAD = 2;
        public const int FAILED_TO_PARSE = 3;
        public const int FAILED_TO_APPLY_MIGRATIONS = 4;
        public const int SQL_FILE_NOT_FOUND = 5;
        public const int FAILED_TO_RUN_SQL = 6;
    }
}