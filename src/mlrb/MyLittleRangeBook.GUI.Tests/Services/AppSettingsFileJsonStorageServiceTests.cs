using System.Text.Json.Nodes;
using MyLittleRangeBook.GUI.Services;
using Shouldly;

namespace MyLittleRangeBook.GUI.Tests.Services
{
    public class AppSettingsFileJsonStorageServiceTests
    {
        const string JSON1 = """
                             {
                               "ConnectionStrings": {
                                 "PostgresqlConnection": "Host=localhost;Port=5432;Database=MyLittleRangeBook;Username=USERNAME;Password=PASSWORD",
                                 "SqliteConnection": "Data Source=mlrb.db"
                               },
                               "Logging": {
                                 "LogLevel": {
                                   "Default": "Information",
                                   "Microsoft.Hosting.Lifetime": "Information"
                                 }
                               },
                               "GuiApp": {
                                 "AccentColor": "#FF3578E5",
                                 "AppTheme": "Light"
                               }
                             }
                             """;

        const string JSON2 = """
                             {
                               "ConnectionStrings": {
                                 "PostgresqlConnection": "Host=localhost;Port=5432;Database=MyLittleRangeBook;Username=USERNAME;Password=PASSWORD",
                                 "SqliteConnection": "Data Source=mlrb.db"
                               },
                               "Logging": {
                                 "LogLevel": {
                                   "Default": "Information",
                                   "Microsoft.Hosting.Lifetime": "Information"
                                 }
                               }
                             }
                             """;


        [Fact]
        public async Task AppSettings_mssing_GuiApp_Should_return_EmptyString()
        {
            string file = Path.GetTempFileName();
            await File.WriteAllTextAsync(file, JSON2);

            try
            {
                AppSettingsFileStorageService sut = new AppSettingsFileStorageService().SettingsFile(file);
                string? x = await sut.ReadAsync();
                x.ShouldBeEmpty();
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public async Task AppSettings_has_GuiApp_Should_return_JSON()
        {
            string file = Path.GetTempFileName();
            await File.WriteAllTextAsync(file, JSON1);

            try
            {
                AppSettingsFileStorageService sut = new AppSettingsFileStorageService().SettingsFile(file);
                string? x = await sut.ReadAsync();
                x.ShouldNotBeNullOrEmpty();

                var node = JsonNode.Parse(x);
                node!["AccentColor"]!.ToString().ShouldBe("#FF3578E5");
                node!["AppTheme"]!.ToString().ShouldBe("Light");
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
