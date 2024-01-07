using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class CryptoServiceTests
{
    private Mock<IHttpService> _mockHttpService = null!;
    private CryptoService _cryptoService = null!;

    [SetUp]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpService>();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("ByBit:ApiUrl", "https://api.bybit.com/v2/public/tickers?symbol="),
        });
        var configuration = configurationBuilder.Build();

        _cryptoService = new CryptoService(_mockHttpService.Object, configuration);
    }

    [Test]
    public async Task GetCurrentCryptoPriceAsync_ReturnsCurrentPrice()
    {
        // Arrange
        string symbol = "BTC";
        string physicalCurrency = "USDT";
        const string jsonResponse = "{\"result\": [{\"last_price\": \"50000\"}]}";
        const string expectedPrice = "50000";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));

        // Act
        var (success, result) = await _cryptoService.GetCryptoPriceAsync(symbol, physicalCurrency);

        // Assert
        Assert.IsTrue(success);
        Assert.That(result, Is.EqualTo(expectedPrice));
    }

    [Test]
    public async Task GetCurrentCryptoPriceAsync_ApiError_ReturnsErrorMessage()
    {
        // Arrange
        string symbol = "BTC";
        string physicalCurrency = "USDT";
        const string expectedErrorMessage = "API Error";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(false, expectedErrorMessage));

        // Act
        var (success, result) = await _cryptoService.GetCryptoPriceAsync(symbol, physicalCurrency);

        // Assert
        Assert.IsFalse(success);
        Assert.That(result, Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    public async Task GetCurrentCryptoPriceAsync_InvalidJson_ReturnsFallbackMessage()
    {
        // Arrange
        string symbol = "BTC";
        string physicalCurrency = "USDT";
        const string invalidJsonResponse = "Invalid JSON";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, invalidJsonResponse));

        // Act
        var (success, result) = await _cryptoService.GetCryptoPriceAsync(symbol, physicalCurrency);

        // Assert
        Assert.IsFalse(success);
        Assert.That(result, Is.EqualTo($"Could not fetch price of {symbol}..."));
    }

    [Test]
    public async Task GetCurrentCryptoPriceAsync_MissingLastPrice_ReturnsFallbackMessage()
    {
        // Arrange
        string symbol = "BTC";
        string physicalCurrency = "USDT";
        const string jsonResponseMissingLastPrice = "{\"result\": [{}]}";
        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<object>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponseMissingLastPrice));

        // Act
        var (success, result) = await _cryptoService.GetCryptoPriceAsync(symbol, physicalCurrency);

        // Assert
        Assert.IsFalse(success);
        Assert.That(result, Is.EqualTo($"Could not fetch price of {symbol}..."));
    }
}
