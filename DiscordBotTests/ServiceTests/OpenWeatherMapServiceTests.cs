using System.Net;
using System.Reflection;
using DiscordBot.Interfaces;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class OpenWeatherMapServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpService>();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("Watch2Gether:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("Watch2Gether:CreateRoomUrl",
                "https://api.watch2gether.com/rooms/create"),
            new KeyValuePair<string, string?>("Watch2Gether:ShowRoomUrl", "https://w2g.tv/rooms/")
        });
        var configuration = configurationBuilder.Build();

        _openWeatherMapService = new OpenWeatherMapService(_mockHttpService.Object, configuration);
    }

    private Mock<IHttpService> _mockHttpService = null!;
    private OpenWeatherMapService _openWeatherMapService = null!;

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

        const string? jsonResponse =
            "{\"name\": \"Berlin\",\"weather\": [{\"description\": \"light rain\",\"icon\": \"10n\"}],\"main\": {\"temp\": 10.55,\"feels_like\": 3.99,\"humidity\": 76},\"wind\": {\"speed\": 5.5}}";

        var expectedResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccessStatusCode = true,
            Content = jsonResponse
        };

        // Create an in-memory configuration for testing purposes
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiUrl",
                "https://api.openweathermap.org/data/2.5/weather?q=")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenWeatherMapService(_mockHttpService.Object, configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponse(true, jsonResponse));


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
            "No OpenWeatherMap Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiKey", ""),
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiUrl",
                "https://api.openweathermap.org/data/2.5/weather?q=")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenWeatherMapService(_mockHttpService.Object, configuration);

        // Set the OpenWeatherMapApiKey to an empty string
        typeof(OpenWeatherMapService)
            .GetField("OpenWeatherMapApiKey", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, string.Empty);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponse(false, string.Empty));

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
        const string? expectedMessage = "Failed to fetch weather data. Please check the city name and try again.";
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiKey", "testKey"),
            new KeyValuePair<string, string?>("OpenWeatherMap:ApiUrl",
                "https://api.openweathermap.org/data/2.5/weather?q=")
        });
        var configuration = configurationBuilder.Build();

        var service = new OpenWeatherMapService(_mockHttpService.Object, configuration);

        _mockHttpService.Setup(x => x.GetResponseFromUrl(It.IsAny<string>(), It.IsAny<Method>(), It.IsAny<string>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>()))
            .ReturnsAsync(new HttpResponse(false, expectedMessage));

        // Act
        var result = await service.GetWeatherAsync(city);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.That(result.Message,
            Is.EqualTo(expectedMessage));
        Assert.IsNull(result.weatherData);
    }
}