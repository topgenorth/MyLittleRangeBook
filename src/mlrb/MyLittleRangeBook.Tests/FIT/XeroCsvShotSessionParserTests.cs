using MyLittleRangeBook.FIT;
using MyLittleRangeBook.FIT.Model;
using Serilog;
using Shouldly;
using NSubstitute;

namespace MyLittleRangeBook.Tests.FIT
{
    public class XeroCsvShotSessionParserTests
    {
        private const string CSV_CONTENT = 
@"Rifle Bullet 36.0 gr
?#,Speed (FPS), Avg (FPS),KE (FT-LBS),Power Factor (kgrft/s),Time,Clean Bore,Cold Bore,Shot Notes
1,""1,369.8"",-0.8,150.0,49.3,13:32:27,,,
2,""1,402.0"",31.5,157.1,50.5,13:32:45,,,
3,""1,374.0"",3.4,150.9,49.5,13:32:57,,,
4,""1,369.1"",-1.5,149.8,49.3,13:33:07,,,
5,""1,391.6"",21.1,154.8,50.1,13:38:00,,,
6,""1,374.1"",3.5,150.9,49.5,13:38:18,,,
7,""1,361.0"",-9.5,148.0,49.0,13:38:26,,,
8,""1,332.2"",-38.4,141.8,48.0,13:38:33,,,
9,""1,361.3"",-9.3,148.1,49.0,13:38:43,,,
-,,,,,,
AVERAGE SPEED,""1,370.6"",,,,,
AVERAGE POWER FACTOR,49.3,,,,,
STD DEV,18.6,,,,,
SPREAD,69.8,,,,,
Projectile Weight (GRAINS),36.0,,,,,
AVG KINETIC ENERGY,150.2,,,,,
Session Note,This is a test session,,,,,
-,,,,,,";

        [Fact]
        public async Task ParseCsvFileAsync_ShouldParseSampleCorrectly()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var parser = new XeroCsvShotSessionParser(logger);
            var tempFile = Path.GetTempFileName() + "Rifle_Bullet_2026-07-05_13-30-54.csv";
            await File.WriteAllTextAsync(tempFile, CSV_CONTENT);

            try
            {
                // Act
                var result = await parser.ParseCsvFileAsync(tempFile, CancellationToken.None);

                // Assert
                result.IsSuccess.ShouldBeTrue();
                var session = result.Value;
                session.ProjectileType.ShouldBe("Rifle Bullet");
                session.ProjectileWeight.ShouldBe(36);
                session.ProjectileUnits.ShouldBe("gr");
                session.ShotCount.ShouldBe(9);
                session.AverageSpeed.ShouldBe(1370);
                session.ExtremeSpread.ShouldBe(69);
                session.StandardDeviation.ShouldBe(18.6);
                session.Notes.ShouldBe("This is a test session");
                session.VelocityUnits.ShouldBe("fps");

                // Check first shot
                var firstShot = session.Shots[1];
                firstShot.Speed.Value.ShouldBe(1370); // Rounded 1369.8
                firstShot.Speed.Units.ShouldBe("fps");

                // Check last shot
                var lastShot = session.Shots[9];
                lastShot.Speed.Value.ShouldBe(1361); // Rounded 1361.3
                
                // Check date from filename
                session.DateTimeUtc.Year.ShouldBe(2026);
                session.DateTimeUtc.Month.ShouldBe(7);
                session.DateTimeUtc.Day.ShouldBe(5);
                session.DateTimeUtc.Hour.ShouldBe(13);
                session.DateTimeUtc.Minute.ShouldBe(30);
                session.DateTimeUtc.Second.ShouldBe(54);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
