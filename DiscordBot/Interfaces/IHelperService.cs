namespace DiscordBot.Interfaces;

public interface IHelperService
{
    public Task<string> GetCurrentBitcoinPriceAsync();

    public Task<string> GetRandomDeveloperExcuseAsync();
}