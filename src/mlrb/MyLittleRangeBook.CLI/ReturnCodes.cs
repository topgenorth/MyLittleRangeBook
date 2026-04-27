namespace MyLittleRangeBook.CLI
{
    public static class ReturnCodes
    {
        /// <summary>
        ///     Command was successful.
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        ///     The command failed for any reason.
        /// </summary>
        public const int FAILURE = 1;

        /// <summary>
        ///     The command was running, but was cancelled by the user.
        /// </summary>
        public const int COMMAND_CANCELLED = 2;


        #region SQLite specific return codes
        /// <summary>
        ///     Could not find the SQLITE database file.
        /// </summary>
        public const int SQL_SQLITE_DATABASE_FILE_NOT_FOUND = 301;
        #endregion


        public const int RANGE_EVENT_FAILED_TO_CREATE = 401;
        public const int RANGE_EVENT_FAILED_TO_CREATE_RANGE_EVENT_TASK_CANCELLED = 402;


        #region General SQL return codes.
        /// <summary>
        ///     There was a problem writing data to the database
        /// </summary>
        public const int SQL_FAILED_TO_WRITE_TO_DATABASE = 101;

        /// <summary>
        ///     THere was a problem running an SQL script against the database.
        /// </summary>
        public const int SQL_FAILED_TO_RUN_SCRIPT = 102;

        /// <summary>
        ///     There was a problem applying database migrations.
        /// </summary>
        public const int SQL_FAILED_TO_APPLY_MIGRATIONS = 103;

        /// <summary>
        /// Could not find the SQL script file.
        /// </summary>
        public const int SQL_SCRIPT_FILE_NOT_FOUND = 104;
        #endregion

        #region FIT File return codes
        /// <summary>
        ///     Could not find the FIT file.
        /// </summary>
        public const int FIT_FILE_NOT_FOUND = 201;

        /// <summary>
        ///     There was an issue reading the FIT file.
        /// </summary>
        public const int FIT_FILE_READ_FAILURE = 202;

        /// <summary>
        ///     There was an issue parsing the FIT file.
        /// </summary>
        public const int FIT_FILE_PARSE_FAILURE = 203;
        #endregion;
    }
}
