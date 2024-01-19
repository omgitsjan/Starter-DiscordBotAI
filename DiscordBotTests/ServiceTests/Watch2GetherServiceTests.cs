using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class Watch2GetherServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpService>();

        // Create an in-memory configuration for testing purposes
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("Watch2Gether:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("Watch2Gether:CreateRoomUrl",
                "https://api.watch2gether.com/rooms/create"),
            new KeyValuePair<string, string?>("Watch2Gether:ShowRoomUrl", "https://w2g.tv/rooms/")
        });
        var configuration = configurationBuilder.Build();

        _watch2GetherService = new Watch2GetherService(_mockHttpService.Object, configuration);
    }

    private Watch2GetherService _watch2GetherService = null!;
    private Mock<IHttpService> _mockHttpService = null!;

    [Test]
    public async Task CreateRoom_SuccessfulRequest_ReturnsSuccessAndRoomUrl()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        const string streamKey = "AbCdEfGhIjKlMnOpQrStUvWxYz";
        var jsonResponse = JsonConvert.SerializeObject(new { streamkey = streamKey });

        _mockHttpService
            .Setup(x => x.GetResponseFromUrl(
                It.IsAny<string>(),
                It.IsAny<Method>(),
                It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>?>(),
                It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        Assert.IsTrue(success);
        Assert.That(result, Is.EqualTo($"https://w2g.tv/rooms/{streamKey}"));
    }

    [Test]
    public async Task CreateRoom_EmptyResponse_ReturnsNoResponseError()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        const string? expectedErrorMessage = "No response from Watch2Gether";

        _mockHttpService
            .Setup(x => x.GetResponseFromUrl(
                It.IsAny<string>(),
                It.IsAny<Method>(),
                It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>?>(),
                It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, expectedErrorMessage));

        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        Assert.IsFalse(success);
        Assert.That(result, Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    public async Task CreateRoom_DeserializationError_ReturnsDeserializationError()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        const string? errorMessage = "Failed to deserialize response from Watch2Gether";

        _mockHttpService
            .Setup(x => x.GetResponseFromUrl(
                It.IsAny<string>(),
                It.IsAny<Method>(),
                It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>?>(),
                It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, errorMessage));

        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        Assert.IsFalse(success);
        Assert.That(result, Is.EqualTo(errorMessage));
    }
}