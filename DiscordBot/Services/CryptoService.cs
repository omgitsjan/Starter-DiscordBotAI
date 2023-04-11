using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services;

public class CryptoService : ICryptoService
{
    /// <summary>
    ///     Url to the Bitcoin Price Api
    /// </summary>
    private string? _byBitApiUrlBtc;

    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;

    public CryptoService(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets the current Bitcion price from bybit public api
    /// </summary>
    /// <returns>The current price from bitcoin as BTCUSD string</returns>
    public async Task<string> GetCurrentBitcoinPriceAsync()
    {
        _byBitApiUrlBtc = _configuration["ByBit:ApiUrlBtc"] ?? string.Empty;

        if (string.IsNullOrEmpty(_byBitApiUrlBtc))
        {
            const string errorMessage =
                "No ByBit Api Url was provided, please contact the Developer to add a valid Api Url!";
            Program.Log($"{nameof(GetCurrentBitcoinPriceAsync)}: " + errorMessage, LogLevel.Error);
            return errorMessage;
        }

        var response = await _httpService.GetResponseFromURL(_byBitApiUrlBtc);

        if (!response.IsSuccessStatusCode)
        {
            return response.Content;
        }

        var json = JObject.Parse(response.Content ?? "{}");

        return json["result"]?[0]?["last_price"]?.Value<string>() ?? "Could not fetch current Bitcoin price...";
    }
}