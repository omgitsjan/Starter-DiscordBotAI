namespace DiscordBot.Interfaces
{
    public interface ICryptoService
    {
        public Task<Tuple<bool, string>> GetCryptoPriceAsync(string symbol);
    }
}