namespace MyLittleRangeBook.Tests
{
    public class FileExtensionsTests
    {
        const string TestFileName = "test.txt";
        readonly string _testDirectory = Path.GetTempPath();

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
