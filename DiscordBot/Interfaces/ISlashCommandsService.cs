using DiscordBot.Wrapper;

namespace DiscordBot.Interfaces;

public interface ISlashCommandsService
{
    Task PingSlashCommandAsync(IInteractionContextWrapper context);
    Task ChatSlashCommandAsync(IInteractionContextWrapper context, string text);
    Task ImageSlashCommandAsync(IInteractionContextWrapper context, string text);
    Task Watch2GetherSlashCommandAsync(IInteractionContextWrapper context, string url);
    Task WeatherSlashCommandAsync(IInteractionContextWrapper context, string city);
}