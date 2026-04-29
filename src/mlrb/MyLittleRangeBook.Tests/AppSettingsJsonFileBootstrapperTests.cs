using System.Text.Json.Nodes;
using FluentResults;
using MyLittleRangeBook.Config;
using Shouldly;

namespace MyLittleRangeBook
{
    public class AppSettingsJsonFileBootstrapperTests : IDisposable
    {
        readonly string _appSettingsFile;
        readonly JsonNode _appSettingsJson;
        readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"mlrb-tests-{Guid.NewGuid():N}");

        public AppSettingsJsonFileBootstrapperTests()
        {
            _appSettingsFile = Path.Combine(_tempDirectory, "appsettings-test.json");
            IAppSettingsBootstrapper bootstrapper = new AppSettingsJsonFileBootstrapper()
                    .AddBootStrapper(AppSettingsJsonFileBootstrapper.LoggingSectionBootstrapper)
                ;
            Result result = bootstrapper.EnsureAppSettingsExistsAsync(_appSettingsFile).Result;
            result.IsSuccess.ShouldBeTrue();

            string content = File.ReadAllText(_appSettingsFile);
            _appSettingsJson = JsonNode.Parse(content)!;
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Fact]
        public void EnsureAppSettingsExistsAsync_ShouldCreateFile()
        {
            File.Exists(_appSettingsFile).ShouldBeTrue();
        }


        [Fact]
        public void EnsureAppSettingsExistsAsync_Should_Have_LoggingSection()
        {
            _appSettingsJson.ShouldNotBeNull();
            _appSettingsJson["Logging"].ShouldNotBeNull();
            _appSettingsJson["Logging"]!["LogLevel"].ShouldNotBeNull();
            _appSettingsJson["Logging"]!["LogLevel"]!["Default"]!.GetValue<string>().ShouldBe("Error");
        }
    }
}
