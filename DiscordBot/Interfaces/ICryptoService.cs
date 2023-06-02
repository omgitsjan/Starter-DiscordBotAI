namespace DiscordBot.Interfaces
{
    public interface ICryptoService
    {
        public Task<string> GetCurrentBitcoinPriceAsync();
    }
}