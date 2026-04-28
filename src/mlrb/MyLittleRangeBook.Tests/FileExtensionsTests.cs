using Shouldly;

namespace MyLittleRangeBook
{
    public class FileExtensionsTests
    {
        const string TestFileName = "test.txt";
        readonly string _testDirectory = Path.GetTempPath();


        [Theory]
        [InlineData("0.9.0.101+0e971a3.0e971a30e99d9114d2f90ca38b6feab611685ac0", "0.9.0.101+0e971a3")]
        [InlineData("0.9.0.101+0e971a3", "0.9.0.101+0e971a3")]
        [InlineData("0.9.0.101", "0.9.0.101")]
        public void CleanAssemblyVersionTest(string assemblyVersion, string expectedVersion)
        {
            string result = FileExtensions.RemoveFullGitShaFromInformationalVersion(assemblyVersion);

            result.ShouldBe(expectedVersion);
        }

        [Fact]
        public void InjectEnvironmentIntoFileName_ShouldReturnOriginalFileInfo_WhenEnvironmentIsProduction()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
            var fileInfo = new FileInfo(Path.Combine(_testDirectory, TestFileName));

            // Act
            FileInfo result = fileInfo.InjectEnvironmentIntoFileName();

            // Assert
            Assert.Equal(fileInfo.FullName, result.FullName);
        }

        [Fact]
        public void InjectEnvironmentIntoFileName_ShouldReturnOriginalFileInfo_WhenEnvironmentIsMissing()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            var fileInfo = new FileInfo(Path.Combine(_testDirectory, TestFileName));

            // Act
            FileInfo result = fileInfo.InjectEnvironmentIntoFileName();

            // Assert
            Assert.Equal(fileInfo.FullName, result.FullName);
        }

        [Fact]
        public void InjectEnvironmentIntoFileName_ShouldReturnOriginalFileInfo_WhenEnvironmentIsEmpty()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "");
            var fileInfo = new FileInfo(Path.Combine(_testDirectory, TestFileName));

            // Act
            FileInfo result = fileInfo.InjectEnvironmentIntoFileName();

            // Assert
            Assert.Equal(fileInfo.FullName, result.FullName);
        }

        [Fact]
        public void InjectEnvironmentIntoFileName_ShouldInjectEnvironment_WhenEnvironmentIsDevelopment()
        {
            // Arrange
            const string env = "Development";
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", env);
            var fileInfo = new FileInfo(Path.Combine(_testDirectory, TestFileName));
            var expectedName = $"test-{env}.txt";

            // Act
            FileInfo result = fileInfo.InjectEnvironmentIntoFileName();

            // Assert
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(Path.Combine(_testDirectory, expectedName), result.FullName);
        }

        [Fact]
        public void InjectEnvironmentIntoFileName_ShouldInjectEnvironment_WhenEnvironmentIsStaging()
        {
            // Arrange
            const string ENV = "Staging";
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", ENV);
            var fileInfo = new FileInfo(Path.Combine(_testDirectory, TestFileName));
            var expectedName = $"test-{ENV}.txt";

            // Act
            FileInfo result = fileInfo.InjectEnvironmentIntoFileName();

            // Assert
            Assert.Equal(expectedName, result.Name);
            Assert.Equal(Path.Combine(_testDirectory, expectedName), result.FullName);
        }
    }
}
