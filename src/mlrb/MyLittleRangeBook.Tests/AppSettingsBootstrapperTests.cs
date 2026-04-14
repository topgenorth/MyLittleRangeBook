using System.Text.Json;
using MyLittleRangeBook.Config;

namespace MyLittleRangeBook.Tests
{
    public class AppSettingsBootstrapperTests : IDisposable
    {
        readonly string _oldEnvironment;

        public AppSettingsBootstrapperTests()
        {
            _oldEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _oldEnvironment);
        }

        [Fact]
        public async Task EnsureAppSettingsExistsAsync_ShouldCreateFile_WithCorrectContent()
        {
            var bootstrapper = new AppSettingsBootstrapper();
            string filePath = await bootstrapper.EnsureAppSettingsExistsAsync();

            try
            {
                Assert.True(File.Exists(filePath));
                Assert.Contains("appsettings-Development.json", filePath);

                string content = await File.ReadAllTextAsync(filePath);
                var json = JsonDocument.Parse(content);
                string? connectionString = json.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("SqliteConnection")
                    .GetString();

                string expectedDbPath = ConfigurationExtensions.DefaultSqliteDatabaseName();
                Assert.Equal($"Data Source={expectedDbPath}", connectionString);

                Assert.True(json.RootElement.TryGetProperty("Logging", out _));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Fact]
        public async Task EnsureAppSettingsExistsAsync_ShouldNotOverwrite_IfFileExists()
        {
            var bootstrapper = new AppSettingsBootstrapper();
            string filePath = await bootstrapper.EnsureAppSettingsExistsAsync();

            try
            {
                var originalContent = "{\"Preserve\": true}";
                await File.WriteAllTextAsync(filePath, originalContent);

                string returnedPath = await bootstrapper.EnsureAppSettingsExistsAsync();

                Assert.Equal(filePath, returnedPath);
                string currentContent = await File.ReadAllTextAsync(filePath);
                Assert.Equal(originalContent, currentContent);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Fact]
        public async Task EnsureAppSettingsExistsAsync_ShouldRespectProductionEnvironment()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var bootstrapper = new AppSettingsBootstrapper();
            string filePath = await bootstrapper.EnsureAppSettingsExistsAsync();

            try
            {
                Assert.True(File.Exists(filePath));
                Assert.EndsWith("appsettings.json", filePath);
                Assert.DoesNotContain("Production", filePath);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
