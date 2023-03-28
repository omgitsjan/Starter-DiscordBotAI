using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiscordBot.Services;

public class OpenAiService
{
    /// <summary>
    ///     Api Key to access OpenAi Apis like ChatGPT - (REPLACE THIS WITH YOUR API KEY)
    /// </summary>
    private const string OpenAiApiKey = "";

    /// <summary>
    ///     Url to the ChatGPT Api
    /// </summary>
    private const string ChatGptApiUrl = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    ///     Url to the Dall-E Api
    /// </summary>
    private const string DalleApiUrl = "https://api.openai.com/v1/images/generations";

    /// <summary>
    ///     The method uses the RestClient class to send a request to the ChatGPT API, passing the user's message as the
    ///     prompt and sends the response into the Chat
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Boolean indicating whether the request was successful, also the message itself</returns>
    internal static async Task<Tuple<bool, string>> ChatGpt(string message)
    {
        // Holds the response from the API.
        string responseText;

        // Use to indicate if the operation was successful or not
        var success = false;

        if (string.IsNullOrEmpty(OpenAiApiKey))
        {
            responseText = "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
            Program.Log($"{nameof(ChatGpt)}: " + responseText, LogLevel.Error);
            return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
        }

        // Create a new RestClient instance
        var client = new RestClient(ChatGptApiUrl);

        // Create a new RestRequest instance
        var request = new RestRequest("", Method.Post);

        // Set the request headers
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {OpenAiApiKey}");

        // Create the request data
        var data = new
        {
            // The prompt with model gpt-3.5-turbo
            // (the same model used in ChatGPT)
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = message } }
        };

        // Serialzie it via JsonSerializer
        var jsonDataString = JsonConvert.SerializeObject(data);

        // Add the request data to the request body
        request.AddJsonBody(jsonDataString);

        // Send the request and get the response
        var response = await client.ExecuteAsync(request);

        // Check the status code of the response
        if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
        {
            // Get the response text from the API
            responseText =
                JsonConvert.DeserializeObject<dynamic>(response.Content)?["choices"][0]["message"]["content"] ?? "";

            if (string.IsNullOrEmpty(responseText))
            {
                responseText = "Could not deserialize response from ChatGPT API!";
                Program.Log($"{nameof(ChatGpt)}: " + responseText, LogLevel.Error);
            }

            success = true;
        }
        else
        {
            // Get the ErrorMessage from the API
            responseText = response.ErrorMessage ?? "Unknown error occurred";
            Program.Log($"{nameof(ChatGpt)}: " + responseText, LogLevel.Error);
        }

        return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
    }

    /// <summary>
    ///     The method uses the RestClient class to send a request to the Dall-E API, passing the user's message as the
    ///     prompt and sends an image to the Chat
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Boolean indicating whether the request was successful, also the message itself</returns>
    internal static async Task<Tuple<bool, string>> DallE(string message)
    {
        // Holds the response from the API.
        string responseText;

        // Use to indicate if the operation was successful or not
        var success = false;

        if (string.IsNullOrEmpty(OpenAiApiKey))
        {
            responseText = "No OpenAI Api Key was provided, please contact the Developer to add a valid Api Key!";
            Program.Log($"{nameof(DallE)}: " + responseText, LogLevel.Error);
            return new Tuple<bool, string>(success, responseText.TrimStart('\n'));
        }

        // Create a new RestClient instance
        var client = new RestClient(DalleApiUrl);

        // Create a new RestRequest instance
        var request = new RestRequest("", Method.Post);

        // Set the request headers
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {OpenAiApiKey}");

        // Create the request data
        var data = new
        {
            // The prompt is everything after the !image command
            //model = "image-alpha-001",
            prompt = message,
            n = 1,
            size = "1024x1024"
        };

        // Serialzie it via JsonSerializer
        var jsonData = JsonSerializer.Serialize(data);

        // Add the request data to the request body
        request.AddJsonBody(jsonData);

        // Send the request and get the response
        var response = await client.ExecuteAsync(request);

        // Check the status code of the response
        if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
        {
            // Get the image URL from the API response
            var imageUrl = JsonConvert.DeserializeObject<dynamic>(response.Content)?["data"][0]["url"];
            responseText = $"Here is your generated image: {imageUrl}";

            if (string.IsNullOrEmpty(imageUrl))
            {
                responseText = "Could not deserialize response from Dall-E API!";
                Program.Log($"{nameof(DallE)}: " + responseText, LogLevel.Error);
                return new Tuple<bool, string>(success, responseText);
            }

            // Log the successful API response
            Program.Log(
                $"{nameof(DallE)}: Received a successful response from the Dall-E API. Generated image URL: {imageUrl}");
            success = true;
        }
        else
        {
            // Get the ErrorMessage from the API
            responseText = response.ErrorMessage ?? "Unknown error occurred";

            // Log the failed API response
            Program.Log(
                $"{nameof(DallE)}: Received a failed response from the Dall-E API. Error message: {responseText}",
                LogLevel.Error);
        }

        return new Tuple<bool, string>(success, responseText);
    }
}