using static MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook.Config
{
    public class AppSettingsBootstrapper
    {
        const string DefaultAppSettingsJson = @"{}";

        /// <summary>
        ///     Ensures that the appsettings.json file exists in the user's settings directory. If it
        ///     does not, then it is created with default values.
        /// </summary>
        /// <remarks>
        ///     In the case of a staging or development environment, the filename will have the
        ///     environment name appended to it.
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns>The name of the appsettings.json file.</returns>
        public async Task<string> EnsureAppSettingsExistsAsync(
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(DefaultUserSettingsDirectory);
            FileInfo appSettingsInfo = new FileInfo(Path.Combine(DefaultUserSettingsDirectory, "appsettings.json"))
                .InjectEnvironmentIntoFileName();

            if (appSettingsInfo.Exists)
            {
                return appSettingsInfo.FullName;
            }

            // var json = JsonSerializer.Serialize(
            //     defaults,
            //     new JsonSerializerOptions { WriteIndented = true });
            //
            // await File.WriteAllTextAsync(settingsPath, json, cancellationToken);

            await File.WriteAllTextAsync(appSettingsInfo.FullName, DefaultAppSettingsJson, cancellationToken);

            return appSettingsInfo.FullName;
        }
    }
}
