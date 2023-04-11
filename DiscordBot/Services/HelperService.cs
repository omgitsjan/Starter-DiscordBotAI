using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DiscordBot.Services;

public class HelperService : IHelperService
{
    private string? _byBitApiUrlBtc;
    private string? _developerExcuseApiUrl;

    private readonly IRestClient _httpClient;
    private readonly IConfiguration _configuration;

    public HelperService(IRestClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
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

        RestRequest request = new(_byBitApiUrlBtc);
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
        _developerExcuseApiUrl = _configuration["DeveloperExcuse:ApiUrl"] ?? string.Empty;

        if (string.IsNullOrEmpty(_developerExcuseApiUrl))
        {
            const string errorMessage =
                "No DeveloperExcuse Api Url was provided, please contact the Developer to add a valid Api Url!";
            Program.Log($"{nameof(GetRandomDeveloperExcuseAsync)}: " + errorMessage, LogLevel.Error);
            return errorMessage;
        }

        RestRequest request = new(_developerExcuseApiUrl);
        var response = await _httpClient.ExecuteAsync(request);
        var jsonString = response.Content;
        var json = JObject.Parse(jsonString ?? "{}");
        return json["text"]?.Value<string>() ?? "Could not fetch current Developer excuse...";
    }
}