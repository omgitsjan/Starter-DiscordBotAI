using DiscordBot.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DiscordBot.Services;

public class HelperService : IHelperService
{
    private readonly IRestClient _httpClient;

    public HelperService(IRestClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    ///     Gets the current Bitcion price from bybit public api
    /// </summary>
    /// <returns>The current price from bitcoin as BTCUSD string</returns>
    public async Task<string> GetCurrentBitcoinPriceAsync()
    {
        var request = new RestRequest("https://api.bybit.com/v2/public/tickers?symbol=BTCUSD");
        var response = await _httpClient.ExecuteAsync(request);
        var jsonString = response.Content;
        var json = JObject.Parse(jsonString ?? "{}");
        return json["result"]?[0]?["last_price"]?.Value<string>() ?? "Could not fetch current Bitcoin price...";
    }

    /// <summary>
    ///     Gets a random dev excuse from the open dev-excuses-api (herokuapp.com)
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetRandomDeveloperExcuseAsync()
    {
        var request = new RestRequest("https://api.devexcus.es/");
        var response = await _httpClient.ExecuteAsync(request);
        var jsonString = response.Content;
        var json = JObject.Parse(jsonString ?? "{}");
        return json["text"]?.Value<string>() ?? "Could not fetch current Developer excuse...";
    }
}