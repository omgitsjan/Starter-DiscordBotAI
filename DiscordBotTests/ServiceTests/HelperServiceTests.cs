using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class HelperServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpService>();

        // Create an in-memory configuration for testing purposes
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("ByBit:ApiUrlBtc", "https://api.bybit.com/v2/public/tickers"),
            new KeyValuePair<string, string?>("DeveloperExcuse:ApiUrl", "https://dev-excuse-api.herokuapp.com/")
        });
        var configuration = configurationBuilder.Build();

        _helperService = new HelperService(_mockHttpService.Object, configuration);
    }

    private Mock<IHttpService> _mockHttpService;
    private HelperService _helperService;

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_ReturnsRandomDeveloperExcuse()
    {
        // Arrange
        const string? jsonResponse = "{\"text\": \"It was a compiler issue.\"}";
        const string expectedExcuse = "It was a compiler issue.";

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await _helperService.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedExcuse));
    }

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_InvalidJson_ReturnsFallbackMessage()
    {
        // Arrange
        const string? invalidJson = "Invalid JSON";
        const string expectedFallbackMessage = "Could not fetch current Developer excuse...";

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, invalidJson));

        // Act
        var result = await _helperService.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedFallbackMessage));
    }

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_NoTextFieldInJson_ReturnsFallbackMessage()
    {
        // Arrange
        const string? jsonResponse = "{\"not_text\": \"It was a compiler issue.\"}";
        const string expectedFallbackMessage = "Could not fetch current Developer excuse...";

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await _helperService.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedFallbackMessage));
    }

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_NonSuccessStatusCode_ReturnsErrorMessage()
    {
        // Arrange
        const string? errorMessage = "Error fetching data.";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, errorMessage));

        // Act
        var result = await _helperService.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.That(result, Is.EqualTo(errorMessage));
    }
}