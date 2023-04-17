using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class CryptoServiceTests
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

        _cryptoService = new CryptoService(_mockHttpService.Object, configuration);
    }

    private Mock<IHttpService> _mockHttpService;
    private CryptoService _cryptoService;

    [Test]
    public async Task GetCurrentBitcoinPriceAsync_ReturnsCurrentBitcoinPrice()
    {
        // Arrange
        const string? jsonResponse = "{\"result\": [{\"last_price\": \"50000\"}]}";
        const string expectedPrice = "50000";

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var result = await _cryptoService.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedPrice));
    }

    [Test]
    public async Task GetCurrentBitcoinPriceAsync_ApiError_ReturnsErrorMessage()
    {
        // Arrange
        const string expectedErrorMessage = "StatusCode: 400 | API Error";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, expectedErrorMessage));

        // Act
        var result = await _cryptoService.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.That(result, Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    public async Task GetCurrentBitcoinPriceAsync_InvalidJson_ReturnsFallbackMessage()
    {
        // Arrange
        const string invalidJsonResponse = "Invalid JSON";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, invalidJsonResponse));

        // Act
        var result = await _cryptoService.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.That(result, Is.EqualTo("Could not fetch current Bitcoin price..."));
    }

    [Test]
    public async Task GetCurrentBitcoinPriceAsync_MissingLastPrice_ReturnsFallbackMessage()
    {
        // Arrange
        const string jsonResponseMissingLastPrice = "{\"result\": [{}]}";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponseMissingLastPrice));

        // Act
        var result = await _cryptoService.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.That(result, Is.EqualTo("Could not fetch current Bitcoin price..."));
    }
}