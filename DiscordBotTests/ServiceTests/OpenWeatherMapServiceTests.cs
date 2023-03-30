using System.Net;
using System.Reflection;
using DiscordBot.Services;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class OpenWeatherMapServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockRestClient = new Mock<IRestClient>();
        _openWeatherMapService = new OpenWeatherMapService(_mockRestClient.Object);
    }

    private Mock<IRestClient> _mockRestClient;
    private OpenWeatherMapService _openWeatherMapService;

    [Test]
    public void GetWeatherAsync_WithValidApiKeyAndCity_ReturnsSuccessAndWeatherData()
    {
        // Arrange
        const string city = "Berlin";
        const string description = "light rain";
        const double temperature = 10.55;
        const int humidity = 76;
        const double windSpeed = 5.5;
        const string expectedMessage =
            "In Berlin, the weather currently: light rain. The temperature is 10.55\u00b0C. The humidity is 76% and the wind speed is 5.5 m/s.";

        const string jsonResponse =
            "{\"name\": \"Berlin\",\"weather\": [{\"description\": \"light rain\",\"icon\": \"10n\"}],\"main\": {\"temp\": 10.55,\"feels_like\": 3.99,\"humidity\": 76},\"wind\": {\"speed\": 5.5}}";

        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccessStatusCode = true,
            Content = jsonResponse
        };

        var service = new OpenWeatherMapService(_mockRestClient.Object)
        {
            OpenWeatherMapApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = service.GetWeatherAsync(city).Result;

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message.Replace(",", "."), Is.EqualTo(expectedMessage.Replace(",", ".")));
        Assert.That(result.weatherData, Is.Not.Null);
        Assert.That(result.weatherData?.City, Is.EqualTo(city));
        Assert.That(result.weatherData?.Description, Is.EqualTo(description));
        Assert.That(result.weatherData?.Temperature, Is.EqualTo(temperature));
        Assert.That(result.weatherData?.Humidity, Is.EqualTo(humidity));
        Assert.That(result.weatherData?.WindSpeed, Is.EqualTo(windSpeed));
    }

    [Test]
    public async Task GetWeatherAsync_WithInvalidApiKey_ReturnsErrorAsync()
    {
        // Arrange
        const string city = "Berlin";
        const string expectedMessage =
            "No OpenWeatherMap Api Key was provided, please contact the Developer to add a valid Api Key!";

        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.Forbidden,
            Content = ""
        };
        var service = new OpenWeatherMapService(_mockRestClient.Object)
        {
            OpenWeatherMapApiKey = ""
        };

        // Set the OpenWeatherMapApiKey to an empty string
        typeof(OpenWeatherMapService)
            .GetField("OpenWeatherMapApiKey", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, string.Empty);

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.GetWeatherAsync(city);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(expectedMessage));
        Assert.That(result.weatherData, Is.Null);
    }

    [Test]
    public async Task GetWeatherAsync_WithInvalidCity_ReturnsError()
    {
        // Arrange
        const string city = "InvalidCity";
        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.NotFound
        };

        var service = new OpenWeatherMapService(_mockRestClient.Object)
        {
            OpenWeatherMapApiKey = "testKey"
        };

        _mockRestClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await service.GetWeatherAsync(city);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Failed to fetch weather data. Please check the city name and try again.", result.Message);
        Assert.IsNull(result.weatherData);
    }
}