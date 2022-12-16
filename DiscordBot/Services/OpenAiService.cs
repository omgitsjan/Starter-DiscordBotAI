using System.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiscordBot.Services;

public class OpenAiService
{
    /// <summary>
    ///     Api Key to access OpenAi Apis like ChatGPT - (Replace this with your OpenAi API key)
    /// </summary>
    private const string OpenAiApiKey = "";

    /// <summary>
    ///     Url to the ChatGpt Api
    /// </summary>
    private const string ChatGptApiUrl = "https://api.openai.com/v1/completions";

    /// <summary>
    ///     Url to the
    /// </summary>
    private const string DalleApiUrl = "https://api.openai.com/v1/images/generations";

    /// <summary>
    ///     The method uses the RestClient class to send a request to the ChatGPT API, passing the user's message as the
    ///     prompt and sends the response into the Chat
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Boolean indicating whether the request was successful</returns>
    internal static async Task<Tuple<bool, string>> ChatGpt(SocketMessage message)
    {
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
            // The prompt is everything after the !chat command
            model = "text-davinci-003",
            prompt = message.Content[4..],
            max_tokens = 256
        };

        var jsonData = JsonSerializer.Serialize(data);

        // Add the request data to the request body
        request.AddJsonBody(jsonData);

        // Send the request and get the response
        var response = await client.ExecuteAsync(request);

        // Holds the response from the API.
        string responseText;
        var success = true;
        // Check the status code of the response
        if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
        {
            // Get the response text from the API
            responseText = JsonConvert.DeserializeObject<dynamic>(response.Content)?["choices"][0]["text"] ??
                           "Could not deserialize response from ChatGPT Api!";
        }
        else
        {
            // Get the ErrorMessage from the API
            responseText = response.ErrorMessage ?? string.Empty;
            success = false;
        }

        // Send the response to the Discord chat
        await message.Channel.SendMessageAsync(responseText);
        return new Tuple<bool, string>(success, responseText);
    }

    /// <summary>
    ///     The method uses the RestClient class to send a request to the Dall-E API, passing the user's message as the
    ///     prompt and sends an image to the Chat
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Boolean indicating whether the request was successful</returns>
    internal static async Task<Tuple<bool, string>> DallE(SocketMessage message)
    {
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
            prompt = message.Content[5..],
            n = 1,
            size = "1024x1024"
        };

        var jsonData = JsonSerializer.Serialize(data);

        // Add the request data to the request body
        request.AddJsonBody(jsonData);

        // Send the request and get the response
        var response = await client.ExecuteAsync(request);

        // Holds the response from the API.
        string responseText;
        var success = false;
        // Check the status code of the response
        if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
        {
            // Get the image URL from the API response
            var imageUrl = JsonConvert.DeserializeObject<dynamic>(response.Content)?["data"][0]["url"];
            responseText = $"Here is the generated image: {imageUrl}";
            success = true;
        }
        else
        {
            // Get the ErrorMessage from the API
            responseText = response.ErrorMessage ?? string.Empty;
        }

        // Send the response to the Discord chat
        await message.Channel.SendMessageAsync(responseText);

        return new Tuple<bool, string>(success, responseText);
    }
}