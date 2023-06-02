namespace DiscordBot.Interfaces
{
    public interface IOpenAiService
    {
        public Task<Tuple<bool, string>> ChatGptAsync(string message);

        public Task<Tuple<bool, string>> DallEAsync(string message);
    }
}