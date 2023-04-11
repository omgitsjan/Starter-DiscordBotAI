using DiscordBot.Interfaces;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services;

public class CryptoService : ICryptoService
{
    /// <summary>
    ///     Url to the Bitcoin Price Api
    /// </summary>
    private const string BitcoinPriceApiUrl = "https://api.bybit.com/v2/public/tickers?symbol=BTCUSD";

    private readonly IHelperService _helperService;

    public CryptoService(IHelperService helperService)
    {
        _helperService = helperService;
    }

    /// <summary>
    ///     Gets the current Bitcion price from bybit public api
    /// </summary>
    /// <returns>The current price from bitcoin as BTCUSD string</returns>
    public async Task<string> GetCurrentBitcoinPriceAsync()
    {
        var response = await _helperService.GetResponseFromURL(BitcoinPriceApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            return response.Content;
        }

        var json = JObject.Parse(response.Content ?? "{}");

        return json["result"]?[0]?["last_price"]?.Value<string>() ?? "Could not fetch current Bitcoin price...";
    }
}