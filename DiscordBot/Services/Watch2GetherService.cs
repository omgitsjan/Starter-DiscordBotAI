using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBot.Services
{
    public class Watch2GetherService : IWatch2GetherService
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpService _httpService;

        /// <summary>
        ///     Api Key to access Watch2Gether Api
        /// </summary>
        private string? _w2GApiKey;

        /// <summary>
        ///     Url to the CreateRoom Api
        /// </summary>
        private string? _w2GCreateRoomUrl;

        /// <summary>
        ///     Url to the Room URL
        /// </summary>
        private string? _w2GShowRoomUrl;

        public Watch2GetherService(IHttpService httpService, IConfiguration configuration)
        {
            _httpService = httpService;
            _configuration = configuration;
        }

        /// <summary>
        ///     Creates a Watch2Gether room for sharing a video.
        /// </summary>
        /// <param name="videoUrl">The URL of the video to be shared.</param>
        /// <returns>
        ///     A Tuple with two values - a boolean indicating whether the request was successful
        ///     and a string message containing either an error message or a Watch2Gether room URL.
        /// </returns>
        public async Task<Tuple<bool, string?>> CreateRoom(string videoUrl)
        {
            // Retrieve the urls and the apikey from the configuration
            _w2GApiKey = _configuration["Watch2Gether:ApiKey"] ?? string.Empty;
            _w2GCreateRoomUrl = _configuration["Watch2Gether:CreateRoomUrl"] ?? string.Empty;
            _w2GShowRoomUrl = _configuration["Watch2Gether:ShowRoomUrl"] ?? string.Empty;

            string? message = "";

            if (string.IsNullOrEmpty(_w2GApiKey) || string.IsNullOrEmpty(_w2GCreateRoomUrl) ||
                string.IsNullOrEmpty(_w2GShowRoomUrl))
            {
                message = "Could not load necessary configuration, please provide a valid configuration";
                Program.Log($"{nameof(CreateRoom)}: " + message, LogLevel.Error);
                return new Tuple<bool, string?>(false, message);
            }

            List<KeyValuePair<string, string>> headers = new()
            {
                new KeyValuePair<string, string>("Content-Type", "application/json"),
                new KeyValuePair<string, string>("Accept", "application/json")
            };

            var data = new
            {
                w2g_api_key = _w2GApiKey,
                share = videoUrl
            };

            HttpResponse response = await _httpService.GetResponseFromUrl(_w2GCreateRoomUrl, Method.Post,
                $"{nameof(CreateRoom)}: No response from Watch2Gether", headers, data);

            message = response.Content;

            // If the response content is null, set the error message and return the result Tuple.
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                try
                {
                    // Deserialize the response content into a dynamic object and extract the streamkey property.
                    dynamic? responseObj = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    message = _w2GShowRoomUrl + responseObj?.streamkey;
                }
                catch (Exception e)
                {
                    // Log any deserialization exceptions to the console.
                    message = "Failed to deserialize response from Watch2Gether";
                    Program.Log($"{nameof(CreateRoom)}: " + message + $" Error: {e.Message}",
                        LogLevel.Error);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                Program.Log($"{nameof(CreateRoom)}: Successfully created Watch2Gether room: {message}");
            }
            else
            {
                Program.Log($"{nameof(CreateRoom)}: Failed to create Watch2Gether room. Error: {message}",
                    LogLevel.Error);
            }

            // Return the result Tuple with success status and message.
            return new Tuple<bool, string?>(response.IsSuccessStatusCode, message);
        }
    }
}