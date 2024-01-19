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
        private string? _byBitApiUrl;

        /// <summary>
        ///     Gets the current Price of a given Cryptocurrency. Default = BTC & USDT
        /// </summary>
        /// <returns>The current price of given Cryptocurrency as string</returns>
        public async Task<Tuple<bool, string>> GetCryptoPriceAsync(string symbol = "BTC", string physicalCurrency = "USDT")
        {
            _byBitApiUrl = configuration["ByBit:ApiUrl"] ?? string.Empty;
            symbol = symbol.ToUpper();
            if (string.IsNullOrEmpty(_byBitApiUrl))
            {
                const string errorMessage =
                    "No ByBit Api Url was provided, please contact the Developer to add a valid Api Url!";
                Program.Log($"{nameof(GetCryptoPriceAsync)}: " + errorMessage, LogLevel.Error);
                return new Tuple<bool, string>(false, errorMessage);
            }

            string requestUrl = _byBitApiUrl + symbol + physicalCurrency;
            Console.WriteLine(requestUrl);
            HttpResponse response = await httpService.GetResponseFromUrl(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new Tuple<bool, string>(false, response.Content ?? "");
            }

            try
            {
                JObject json = JObject.Parse(response.Content ?? "{}");
                string? respString = json["result"]?[0]?["last_price"]?.Value<string>();
                bool success = respString != null;

                if (!success)
                {
                    respString = $"Could not fetch price of {symbol}...";
                }

                Program.Log($"{nameof(GetCryptoPriceAsync)}: {respString} - {success}", LogLevel.Information);
                return new Tuple<bool, string>(success, respString ?? throw new Exception());
            }
            catch (JsonReaderException ex)
            {
                Program.Log($"{nameof(GetCryptoPriceAsync)}: " + ex.Message, LogLevel.Error);
                return new Tuple<bool, string>(false, $"Could not fetch price of {symbol}...");
            }
        }
    }
}