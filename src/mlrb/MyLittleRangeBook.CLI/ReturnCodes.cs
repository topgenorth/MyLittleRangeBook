namespace MyLittleRangeBook.CLI
{
    public static class ReturnCodes
    {
        public const int SUCCESS = 0;
        public const int FAILURE = 1;

        public const int SQL_SQLITE_DATABASE_FILE_NOT_FOUND = 101;
        public const int SQL_FAILED_TO_WRITE_TO_DATABASE = 102;
        public const int SQL_FILE_NOT_FOUND = 103;
        public const int SQL_FAILED_TO_RUN_SCRIPT = 104;
        public const int SQL_FAILED_TO_APPLY_MIGRATIONS = 105;

        public const int FIT_FILE_READ_FAILURE =202;
        public const int FIT_FILE_PARSE_FAILURE = 203;
        public const int FIT_FILE_NOT_FOUND = 201;

        public const int RANGE_EVENT_FAILED_TO_CREATE = 301;
        public const int RANGE_EVENT_FAILED_TO_CREATE_RANGE_EVENT_TASK_CANCELLED = 302;
    }
}
