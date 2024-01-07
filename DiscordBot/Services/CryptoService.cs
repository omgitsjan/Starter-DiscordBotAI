using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DiscordBot.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpService _httpService;

        /// <summary>
        ///     Url to the Bitcoin Price Api
        /// </summary>
        private string? _byBitApiUrl;

        public CryptoService(IHttpService httpService, IConfiguration configuration)
        {
            _httpService = httpService;
            _configuration = configuration;
        }

        /// <summary>
        ///     Gets the current Price of a given Cryptocurrency. Default = BTC & USDT
        /// </summary>
        /// <returns>The current price of given Cryptocurrency as string</returns>
        public async Task<Tuple<bool, string>> GetCryptoPriceAsync(string symbol = "BTC", string physicalCurrency = "USDT")
        {
            _byBitApiUrl = _configuration["ByBit:ApiUrl"] ?? string.Empty;
            symbol = symbol.ToUpper();
            if (string.IsNullOrEmpty(_byBitApiUrl))
            {
                const string errorMessage =
                    "No ByBit Api Url was provided, please contact the Developer to add a valid Api Url!";
                Program.Log($"{nameof(GetCryptoPriceAsync)}: " + errorMessage, LogLevel.Error);
                return new Tuple<bool, string>(false,errorMessage);
            }

            string requestUrl = _byBitApiUrl + symbol + physicalCurrency;
            Console.WriteLine(requestUrl);
            HttpResponse response = await _httpService.GetResponseFromUrl(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new Tuple<bool, string>(false, response.Content ?? "");
            }

            try
            {
                JObject json = JObject.Parse(response.Content ?? "{}");
                string respString =  json["result"]?[0]?["last_price"]?.Value<string>() ?? $"Could not fetch price of {symbol}...";

                bool success = !string.IsNullOrEmpty(respString) && respString != "Could not fetch current Bitcoin price...";

                Program.Log(respString + " - " + success);
                return new Tuple<bool,string>(success, respString);


            } catch (JsonReaderException ex)
            {
                Program.Log($"{nameof(GetCryptoPriceAsync)}: " + ex.Message, LogLevel.Error);               
                return new Tuple<bool, string>(false, $"Could not fetch price of {symbol}...");
            }
        }
    }
}