using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services
{
    public class CryptoService(IHttpService httpService, IConfiguration configuration) : ICryptoService
    {
        /// <summary>
        ///     Url to the Bitcoin Price Api
        /// </summary>
        private string? _byBitApiUrlBtc;

        /// <summary>
        ///     Gets the current Bitcoin price from bybit public api
        /// </summary>
        /// <returns>The current price from bitcoin as BTC-USD string</returns>
        public async Task<string> GetCurrentBitcoinPriceAsync()
        {
            _byBitApiUrlBtc = configuration["ByBit:ApiUrlBtc"] ?? string.Empty;

            if (string.IsNullOrEmpty(_byBitApiUrlBtc))
            {
                const string? errorMessage =
                    "No ByBit Api Url was provided, please contact the Developer to add a valid Api Url!";
                Program.Log($"{nameof(GetCurrentBitcoinPriceAsync)}: " + errorMessage, LogLevel.Error);
                return errorMessage;
            }

            HttpResponse response = await httpService.GetResponseFromUrl(_byBitApiUrlBtc);

            if (!response.IsSuccessStatusCode)
            {
                return response.Content ?? "";
            }

            try
            {
                JObject json = JObject.Parse(response.Content ?? "{}");
                return json["result"]?[0]?["last_price"]?.Value<string>() ?? "Could not fetch current Bitcoin price...";
            }
            catch (JsonReaderException ex)
            {
                Program.Log($"{nameof(GetCurrentBitcoinPriceAsync)}: " + ex.Message, LogLevel.Error);
                return "Could not fetch current Bitcoin price...";
            }
        }
    }
}