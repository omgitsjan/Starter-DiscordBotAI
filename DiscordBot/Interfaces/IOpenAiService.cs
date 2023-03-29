namespace DiscordBot.Interfaces;

public interface IOpenAiService
{
    public Task<Tuple<bool, string>> ChatGpt(string message);

    public Task<Tuple<bool, string>> DallE(string message);
}