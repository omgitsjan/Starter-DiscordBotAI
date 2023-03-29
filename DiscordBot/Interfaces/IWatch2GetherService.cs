namespace DiscordBot.Interfaces;

public interface IWatch2GetherService
{
    public Task<Tuple<bool, string>> CreateRoom(string videoUrl);
}