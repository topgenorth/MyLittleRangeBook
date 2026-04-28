using System.IO;
using System.Threading.Tasks;
using MyLittleRangeBook.Config;

namespace MyLittleRangeBook.GUI.Services
{
    /// <summary>
    ///     Default file system-based settings storage service.
    ///     Persists application settings to the local disk using JSON format.
    ///     On the Browser platform, it behaves as a no-op due to sandbox restrictions.
    /// </summary>
    public class JsonSettingsFileStorageService : ISettingsStorageService
    {
        string SettingsFile => ConfigurationExtensions.DefaultAppSettingsFile.FullName;
        /// <inheritdoc />
        public async Task<string?> ReadAsync()
        {
            try
            {
                // Browser has no access to the file system due to sandbox restrictions
                if (OperatingSystem.IsBrowser())
                {
                    return null;
                }


                return await File.ReadAllTextAsync(SettingsFile);
            }
            catch
            {
                // In production, consider logging any exceptions for debugging
                return null;
            }
        }

        /// <inheritdoc />
        public async Task WriteAsync(string json)
        {
            try
            {
                // Browser has no access to file system due to its sandbox status
                if (OperatingSystem.IsBrowser())
                {
                    return;
                }


                // Save the provided data into our settings file
                await File.WriteAllTextAsync(SettingsFile, json);
            }
            catch
            {
                // For this sample, we ignore exceptions
                // In production, consider logging exceptions for debugging
            }
        }
    }
}
