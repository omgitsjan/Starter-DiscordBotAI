namespace DiscordBot.Models;

public class WeatherData
{
    public string? City { get; set; }
    public string? Description { get; set; }
    public int? Humidity { get; set; }
    public double? WindSpeed { get; set; }
    public double? Temperature { get; set; }
}