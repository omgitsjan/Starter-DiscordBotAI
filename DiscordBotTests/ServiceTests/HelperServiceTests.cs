using DiscordBot.Services;
using Microsoft.Extensions.Logging;

namespace DiscordBotTests.ServiceTests
{
    [TestFixture]
    public class HelperServiceTests
    {
        private HelperService _helperService = null!;
        private const string TestJsonFilePath = "testExcuses.json";

        [SetUp]
        public void Setup()
        {
            // Create a test JSON file with some developer excuses
            const string testJsonContent = "{\"en\": [\"Test excuse 1\", \"Test excuse 2\", \"Test excuse 3\"]}";
            File.WriteAllText(TestJsonFilePath, testJsonContent);
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete the test JSON file after each test
            if (File.Exists(TestJsonFilePath))
            {
                File.Delete(TestJsonFilePath);
            }
        }

        [Test]
        public async Task GetRandomDeveloperExcuseAsync_ReturnsRandomDeveloperExcuse()
        {
            //Arrange
            _helperService = new HelperService(new Logger<HelperService>(new LoggerFactory()), TestJsonFilePath);

            // Act
            var result = await _helperService.GetRandomDeveloperExcuseAsync();

            // Assert
            Assert.That(result, Is.EqualTo("Test excuse 1").Or.EqualTo("Test excuse 2").Or.EqualTo("Test excuse 3"));
        }

        [Test]
        public async Task GetRandomDeveloperExcuseAsync_InvalidJsonFile_ReturnsFallbackMessage()
        {
            // Arrange
            await File.WriteAllTextAsync(TestJsonFilePath, "Invalid JSON");
            _helperService = new HelperService(new Logger<HelperService>(new LoggerFactory()), TestJsonFilePath);

            // Act
            var result = await _helperService.GetRandomDeveloperExcuseAsync();

            // Assert
            Assert.That(result, Is.EqualTo("Could not fetch a Developer excuse. Please check the configuration."));
        }

        [Test]
        public async Task GetRandomDeveloperExcuseAsync_EmptyJsonFile_ReturnsFallbackMessage()
        {
            // Arrange
            await File.WriteAllTextAsync(TestJsonFilePath, "{}");
            _helperService = new HelperService(new Logger<HelperService>(new LoggerFactory()), TestJsonFilePath);

            // Act
            var result = await _helperService.GetRandomDeveloperExcuseAsync();

            // Assert
            Assert.That(result, Is.EqualTo("Could not fetch a Developer excuse. Please check the configuration."));
        }

        [Test]
        public async Task GetRandomDeveloperExcuseAsync_NoJsonFile_ReturnsFallbackMessage()
        {
            // Arrange
            File.Delete(TestJsonFilePath);
            _helperService = new HelperService(new Logger<HelperService>(new LoggerFactory()), TestJsonFilePath);

            // Act
            var result = await _helperService.GetRandomDeveloperExcuseAsync();

            // Assert
            Assert.That(result, Is.EqualTo("Could not fetch a Developer excuse. Please check the configuration."));
        }
    }
}
