using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class OpenAiServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpService>();

        // Create an in-memory configuration for testing purposes
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        _openAiService = new OpenAiService(_mockHttpService.Object, configuration);
    }

    private Mock<IHttpService> _mockHttpService = null!;
    private OpenAiService? _openAiService = null!;

    [Test]
    public async Task ChatGpt_WithValidApiKeyAndMessage_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string responseText = "I'm doing well, thank you!";
        const string? jsonResponse = "{\"choices\": [{\"message\": {\"content\": \"" + responseText + "\"}}]}";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService?.Object ?? throw new InvalidOperationException(),
            configuration);


        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await service.ChatGptAsync(message);

        // Assert
        Assert.IsTrue(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(responseText));
    }

    [Test]
    public async Task ChatGpt_WithInvalidApiKey_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string expectedError =
            "No OpenAI Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", ""),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, string.Empty));

        // Act
        var result = await service.ChatGptAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(expectedError));
    }

    [Test]
    public async Task ChatGpt_WithDeserializationError_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string? jsonResponse = "{\"choices\": [{\"message\": {\"content\": \"\"}}]}";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));


        // Act
        var result = await service.ChatGptAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo("Could not deserialize response from ChatGPT API!"));
    }

    [Test]
    public async Task ChatGpt_WithUnknownError_ReturnsError()
    {
        // Arrange
        const string message = "Hello, how are you?";
        const string? expectedError = "Unknown error occurred (StatusCode: InternalServerError)";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService?.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, expectedError));

        // Act
        var result = await service.ChatGptAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(expectedError));
    }

    [Test]
    public async Task DallE_WithDeserializationError_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string? jsonResponse = "{\"data\": [{}]}";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await service.DallEAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo("Could not deserialize response from Dall-E API!"));
    }

    [Test]
    public async Task DallE_WithUnknownError_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string? expectedError = "Unknown error occurred";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, expectedError));

        // Act
        var result = await service.DallEAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(expectedError));
    }

    [Test]
    public async Task DallE_WithValidApiKeyAndMessage_ReturnsSuccessAndResponse()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string imageUrl = "https://example.com/generated-image.png";
        const string? jsonResponse = "{\"data\": [{\"url\": \"" + imageUrl + "\"}]}";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await service.DallEAsync(message);

        // Assert
        Assert.IsTrue(result.Item1);
        Assert.That(result.Item2, Is.EqualTo($"Here is your generated image: {imageUrl}"));
    }

    [Test]
    public async Task DallE_WithInvalidApiKey_ReturnsError()
    {
        // Arrange
        const string message = "A beautiful landscape";
        const string expectedError =
            "No OpenAI Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenAi:ApiKey", ""),
            new KeyValuePair<string, string?>("OpenAi:ChatGPTApiUrl",
                "https://api.openai.com/v1/engines/davinci-codex/completions"),
            new KeyValuePair<string, string?>("OpenAi:DallEApiUrl", "https://api.openai.com/v1/images/generations")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenAiService(_mockHttpService.Object ?? throw new InvalidOperationException(),
            configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, string.Empty));

        // Act
        var result = await service.DallEAsync(message);

        // Assert
        Assert.IsFalse(result.Item1);
        Assert.That(result.Item2, Is.EqualTo(expectedError));
    }
}