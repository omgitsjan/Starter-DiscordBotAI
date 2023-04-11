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
        _mockRestClient = new Mock<IRestClient>();

        // Create an in-memory configuration for testing purposes
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("Watch2Gether:ApiKey", "testKey"),
            new KeyValuePair<string, string>("Watch2Gether:CreateRoomUrl", "https://api.watch2gether.com/rooms/create"),
            new KeyValuePair<string, string>("Watch2Gether:ShowRoomUrl", "https://w2g.tv/rooms/")
        });
        var configuration = configurationBuilder.Build();

        _watch2GetherService = new Watch2GetherService(_mockRestClient.Object, configuration);
    }

    private Watch2GetherService _watch2GetherService;
    private Mock<IRestClient> _mockRestClient;

    [Test]
    public async Task CreateRoom_SuccessfulRequest_ReturnsSuccessAndRoomUrl()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        const string streamKey = "AbCdEfGhIjKlMnOpQrStUvWxYz";
        var expectedResponse = new RestResponse
        {
            Content = JsonConvert.SerializeObject(new { streamkey = streamKey })
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse)
            .Verifiable();


        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        _mockRestClient.Verify(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default), Times.Once);
        Assert.IsTrue(success);
        Assert.AreEqual($"https://w2g.tv/rooms/{streamKey}", result);
    }

    [Test]
    public async Task CreateRoom_EmptyResponse_ReturnsNoResponseError()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var expectedResponse = new RestResponse { Content = string.Empty };
        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        _mockRestClient.Verify(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default), Times.Once);
        Assert.IsFalse(success);
        Assert.AreEqual("No response from Watch2Gether", result);
    }

    [Test]
    public async Task CreateRoom_DeserializationError_ReturnsDeserializationError()
    {
        // Arrange
        const string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        const string errorMessage = "Failed to deserialize response from Watch2Gether";
        var expectedResponse = new RestResponse { Content = "invalid json" };
        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var (success, result) = await _watch2GetherService.CreateRoom(videoUrl);

        // Assert
        _mockRestClient.Verify(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default), Times.Once);
        Assert.IsFalse(success);
        Assert.AreEqual(errorMessage, result);
    }
}