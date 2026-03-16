using System;
using System.IO;
using System.Threading.Tasks;

namespace MySimpleRangeLog.Services
{
    public class CsvDbService : IDatabaseService
    {
        public const string FILENAME = "rangelog.csv";

        public string GetDatabasePath()
        {
            var settingsDirectory = JsonSettingsFileStorageService.SettingsDirectory;
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            var filePath = Path.Combine(settingsDirectory, FILENAME);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Could not find the file {filePath}.");
            }

            return filePath;
        }


        public Task SaveAsync()
        {
            throw new NotImplementedException("Cannot save to CSV yet.");
        }
    }
}
