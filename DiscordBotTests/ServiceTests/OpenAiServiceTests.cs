using System.Net;
using DiscordBot.Services;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class OpenAiServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockRestClient = new Mock<IRestClient>();
        _openAiService = new OpenAiService(_mockRestClient.Object);
    }

    private Mock<IRestClient> _mockRestClient;
    private OpenAiService _openAiService;

    [Test]
    public async Task ChatGpt_WithValidApiKeyAndMessage_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string responseText = "I'm doing well, thank you!";
        const string jsonResponse = "{\"choices\": [{\"message\": {\"content\": \"" + responseText + "\"}}]}";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = jsonResponse
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.ChatGpt(message);

        // Assert
        Assert.IsTrue(result.Item1);
        Assert.AreEqual(responseText, result.Item2);
    }

    [Test]
    public async Task ChatGpt_WithInvalidApiKey_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string expectedError =
            "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.Unauthorized
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = ""
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.ChatGpt(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual(expectedError, result.Item2);
    }

    [Test]
    public async Task ChatGpt_WithDeserializationError_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string jsonResponse = "{\"choices\": [{\"message\": {\"content\": \"\"}}]}";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = jsonResponse
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.ChatGpt(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual("Could not deserialize response from ChatGPT API!", result.Item2);
    }

    [Test]
    public async Task ChatGpt_WithUnknownError_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string expectedError = "Unknown error occurred";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.InternalServerError,
            ErrorMessage = null
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.ChatGpt(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual(expectedError, result.Item2);
    }

    [Test]
    public async Task DallE_WithDeserializationError_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string jsonResponse = "{\"data\": [{}]}";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = jsonResponse
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.DallE(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual("Could not deserialize response from Dall-E API!", result.Item2);
    }

    [Test]
    public async Task DallE_WithUnknownError_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string expectedError = "Unknown error occurred";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.InternalServerError,
            ErrorMessage = null
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.DallE(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual(expectedError, result.Item2);
    }


    [Test]
    public async Task DallE_WithValidApiKeyAndMessage_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string imageUrl = "https://example.com/generated-image.png";
        var jsonResponse = "{\"data\": [{\"url\": \"" + imageUrl + "\"}]}";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = jsonResponse
        };

        var service = new OpenAiService(_mockRestClient.Object)
        {
            OpenAiApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.DallE(message);

        // Assert
        Assert.IsTrue(result.Item1);
        Assert.AreEqual($"Here is your generated image: {imageUrl}", result.Item2);
    }

    [Test]
    public async Task DallE_WithInvalidApiKey_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string expectedError =
            "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.Unauthorized
        };


        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _openAiService.DallE(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.AreEqual(expectedError, result.Item2);
    }
}