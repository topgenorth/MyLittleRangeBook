namespace MyLittleRangeBook.Config
{

    public class AppSettingsJsonReadError : MlrbBaseError
    {
        public const int APPSETTINGS_FILE_NOT_CREATED = 502;

        public AppSettingsJsonReadError(string appSettingsJsonFile) :
            base($"Could not read the file {appSettingsJsonFile}", APPSETTINGS_FILE_NOT_CREATED)
        {
            Metadata.Add("Filename", appSettingsJsonFile);
        }

        public AppSettingsJsonReadError(string appSettingsJsonFile, Exception ex) :
            this(appSettingsJsonFile)
        {
            CausedBy(ex);
            Metadata.Add("Exception", ex);
        }
    }

    public class AppSettingsJsonCreationError : MlrbBaseError
    {
        public const int APPSETTINGS_FILE_NOT_CREATED = 501;

        public AppSettingsJsonCreationError(string appSettingsJsonFile) :
            base($"Could not create the file {appSettingsJsonFile}", APPSETTINGS_FILE_NOT_CREATED)
        {
            Metadata.Add("Filename", appSettingsJsonFile);
        }

        public AppSettingsJsonCreationError(string appSettingsJsonFile, Exception ex) :
            this(appSettingsJsonFile)
        {
            this.CausedBy(ex);
            Metadata.Add("Exception", ex);
        }
    }
}
