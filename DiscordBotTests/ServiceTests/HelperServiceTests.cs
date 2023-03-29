using DiscordBot.Services;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class HelperServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockRestClient = new Mock<IRestClient>();
        _helperService = new HelperService(_mockRestClient.Object);
    }

    private Mock<IRestClient> _mockRestClient;
    private HelperService _helperService;

    // ...

    [Test]
    public async Task GetCurrentBitcoinPriceAsync_ReturnsCurrentBitcoinPrice()
    {
        // Arrange
        const string jsonResponse = "{\"result\": [{\"last_price\": \"50000\"}]}";
        const string expectedPrice = "50000";

        _mockRestClient
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(new RestResponse { Content = jsonResponse });

        // Act
        var result = await _helperService.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.AreEqual(expectedPrice, result);
    }

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_ReturnsRandomDeveloperExcuse()
    {
        // Arrange
        const string jsonResponse = "{\"text\": \"It was a compiler issue.\"}";
        const string expectedExcuse = "It was a compiler issue.";

        _mockRestClient
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(new RestResponse { Content = jsonResponse });

        // Act
        var result = await _helperService.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.AreEqual(expectedExcuse, result);
    }

    // ...
}