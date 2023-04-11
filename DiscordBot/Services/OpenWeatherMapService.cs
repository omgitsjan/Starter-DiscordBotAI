using DiscordBot.Interfaces;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DiscordBot.Services;

public class OpenWeatherMapService : IOpenWeatherMapService
{
    /// <summary>
    ///     Url to the OpenWeatherMap Api
    /// </summary>
    private string? _openWeatherMapUrl;


    /// <summary>
    ///     Api Key to access OpenWeatherMap Api
    /// </summary>
    private string? _openWeatherMapApiKey;

    private readonly IRestClient _httpClient;
    private readonly IConfiguration _configuration;


    public OpenWeatherMapService(IRestClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, WeatherData? weatherData)> GetWeatherAsync(string city)
    {
        // Retrieve the url and the apikey from the configuration
        _openWeatherMapUrl = _configuration["OpenWeatherMap:ApiKey"] ?? string.Empty;
        _openWeatherMapApiKey = _configuration["OpenWeatherMap:ApiUrl"] ?? string.Empty;

        if (string.IsNullOrEmpty(_openWeatherMapApiKey) || string.IsNullOrEmpty(_openWeatherMapUrl))
        {
            const string errorMessage =
                "No OpenWeatherMap Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";
            Program.Log($"{nameof(GetWeatherAsync)}: " + errorMessage, LogLevel.Error);
            return (false, errorMessage,
                null);
        }


        var requestUrl =
            $"{_openWeatherMapUrl}{Uri.EscapeDataString(city)}&units=metric&appid={_openWeatherMapApiKey}";

        // Initialize a new instance of RestRequest with the Watch2Gether room creation URL and HTTP method.
        var request = new RestRequest("", Method.Post)
        {
            Resource = requestUrl
        };

        // Send the HTTP request asynchronously and await the response.
        var response = await _httpClient.ExecuteAsync(request);


        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Failed to fetch weather data for city '{city}'. StatusCode: {response.StatusCode}";
            Program.Log($"{nameof(GetWeatherAsync)}: " + errorMessage, LogLevel.Error);
            return (false, "Failed to fetch weather data. Please check the city name and try again.",
                null);
        }

        var jsonString = response.Content;
        var json = JObject.Parse(jsonString ?? "");

        var weather = new WeatherData
        {
            City = json["name"]?.Value<string>(),
            Description = json["weather"]?[0]?["description"]?.Value<string>(),
            Temperature = json["main"]?["temp"]?.Value<double>(),
            Humidity = json["main"]?["humidity"]?.Value<int>(),
            WindSpeed = json["wind"]?["speed"]?.Value<double>()
        };

        var message =
            $"In {weather.City}, the weather currently: {weather.Description}. The temperature is {weather.Temperature:F2}°C. " +
            $"The humidity is {weather.Humidity}% and the wind speed is {weather.WindSpeed} m/s.";

        Program.Log($"{nameof(GetWeatherAsync)}: Weather data fetched successfully. Response: " + message);

        return (true, message, weather);
    }
}