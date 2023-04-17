using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBot.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpService _httpService;

        /// <summary>
        ///     Url to the ChatGPT Api
        /// </summary>
        private string? _chatGptApiUrl;

        /// <summary>
        ///     Url to the Dall-E Api
        /// </summary>
        private string? _dalleApiUrl;

        /// <summary>
        ///     Api Key to access OpenAi Apis like ChatGPT
        /// </summary>
        private string? _openAiApiKey;

        public OpenAiService(IHttpService httpService, IConfiguration configuration)
        {
            _httpService = httpService;
            _configuration = configuration;
        }

        /// <summary>
        ///     The method uses the RestClient class to send a request to the ChatGPT API, passing the user's message as the
        ///     prompt and sends the response into the Chat
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Boolean indicating whether the request was successful, also the message itself</returns>
        public async Task<Tuple<bool, string>> ChatGptAsync(string message)
        {
            // Holds the response from the API.
            string? responseText;

            // Use to indicate if the operation was successful or not
            bool success = false;

            // Retrieve the url and the apikey from the configuration
            _openAiApiKey = _configuration["OpenAi:ApiKey"] ?? string.Empty;
            _chatGptApiUrl = _configuration["OpenAi:ChatGPTApiUrl"] ?? string.Empty;

            if (string.IsNullOrEmpty(_openAiApiKey) || string.IsNullOrEmpty(_chatGptApiUrl))
            {
                const string errorMessage =
                    "No OpenAI Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";
                Program.Log($"{nameof(ChatGptAsync)}: " + errorMessage, LogLevel.Error);
                return new Tuple<bool, string>(success, errorMessage);
            }

            if (string.IsNullOrEmpty(_openAiApiKey))
            {
                responseText = "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
                Program.Log($"{nameof(ChatGptAsync)}: " + responseText, LogLevel.Error);
                return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
            }

            List<KeyValuePair<string, string>> headers = new()
            {
                new KeyValuePair<string, string>("Content-Type", "application/json"),
                new KeyValuePair<string, string>("Authorization", $"Bearer {_openAiApiKey}")
            };

            // Create the request data
            var data = new
            {
                // The prompt with model gpt-3.5-turbo
                // (the same model used in ChatGPT)
                model = "gpt-3.5-turbo",
                messages = new[] { new { role = "user", content = message } }
            };

            HttpResponse response = await _httpService.GetResponseFromUrl(_chatGptApiUrl, Method.Post,
                $"{nameof(ChatGptAsync)}: Unknown error occurred", headers, data);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                // Get the response text from the API
                responseText =
                    JsonConvert.DeserializeObject<dynamic>(response.Content)?["choices"][0]["message"]["content"] ?? "";

                if (string.IsNullOrEmpty(responseText))
                {
                    responseText = "Could not deserialize response from ChatGPT API!";
                    Program.Log($"{nameof(ChatGptAsync)}: " + responseText, LogLevel.Error);
                    return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
                }

                success = true;
            }
            else
            {
                responseText = response.Content;
            }

            return new Tuple<bool, string>(success, responseText?.TrimStart('\n') ?? "");
        }

        /// <summary>
        ///     The method uses the RestClient class to send a request to the Dall-E API, passing the user's message as the
        ///     prompt and sends an image to the Chat
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Boolean indicating whether the request was successful, also the message itself</returns>
        public async Task<Tuple<bool, string>> DallEAsync(string message)
        {
            // Holds the response from the API.
            string? responseText;

            // Use to indicate if the operation was successful or not
            bool success = false;

            // Retrieve the url and the apikey from the configuration
            _openAiApiKey = _configuration["OpenAi:ApiKey"] ?? string.Empty;
            _dalleApiUrl = _configuration["OpenAi:DallEApiUrl"] ?? string.Empty;

            if (string.IsNullOrEmpty(_openAiApiKey) || string.IsNullOrEmpty(_dalleApiUrl))
            {
                const string errorMessage =
                    "No OpenAI Api Key/Url was provided, please contact the Developer to add a valid Api Key/Url!";
                Program.Log($"{nameof(DallEAsync)}: " + errorMessage, LogLevel.Error);
                return new Tuple<bool, string>(success, errorMessage);
            }

            if (string.IsNullOrEmpty(_openAiApiKey))
            {
                responseText = "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
                Program.Log($"{nameof(DallEAsync)}: " + responseText, LogLevel.Error);
                return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
            }

            List<KeyValuePair<string, string>> headers = new()
            {
                new KeyValuePair<string, string>("Content-Type", "application/json"),
                new KeyValuePair<string, string>("Authorization", $"Bearer {_openAiApiKey}")
            };

            // Create the request data
            var data = new
            {
                // The prompt is everything after the !image command
                //model = "image-alpha-001",
                prompt = message,
                n = 1,
                size = "1024x1024"
            };

            HttpResponse response = await _httpService.GetResponseFromUrl(_dalleApiUrl, Method.Post,
                $"{nameof(DallEAsync)}: Received a failed response from the Dall-E API.", headers,
                data);

            // Check the status code of the response
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                // Get the image URL from the API response
                dynamic? imageUrl = JsonConvert.DeserializeObject<dynamic>(response.Content)?["data"][0]["url"];
                string imageUrlString = $"{imageUrl}";
                if (string.IsNullOrEmpty(imageUrlString))
                {
                    responseText = "Could not deserialize response from Dall-E API!";
                    Program.Log($"{nameof(DallEAsync)}: " + responseText, LogLevel.Error);
                    return new Tuple<bool, string>(success, responseText);
                }

                responseText = $"Here is your generated image: {imageUrl}";

                // Log the successful API response
                Program.Log(
                    $"{nameof(DallEAsync)}: Received a successful response from the Dall-E API. Generated image URL: {imageUrl}");
                success = true;
            }
            else
            {
                responseText = response.Content;
            }

            return new Tuple<bool, string>(success, responseText ?? "");
        }
    }
}