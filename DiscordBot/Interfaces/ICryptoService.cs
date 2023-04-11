using RestSharp;
using System.Dynamic;

namespace DiscordBot.Interfaces;

public interface ICryptoService
{
    public Task<string> GetCurrentBitcoinPriceAsync();
}