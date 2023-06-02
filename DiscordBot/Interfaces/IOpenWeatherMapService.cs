using DiscordBot.Models;

namespace DiscordBot.Interfaces
{
    public interface IOpenWeatherMapService
    {
        public Task<(bool Success, string Message, WeatherData? weatherData)> GetWeatherAsync(string city);
    }
}