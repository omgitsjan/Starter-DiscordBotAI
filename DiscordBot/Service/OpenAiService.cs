using System.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OpenAI_Discord_Bot.Service;

public class OpenAiService
{
    /// <summary>
    ///     Api Key to access ChatGPT - (Replace this with your ChatGPT API key)
    /// </summary>
    private const string ChatGptApiKey = "";

    /// <summary>
    ///     Url to the ChatGpt Api
    /// </summary>
    private const string ChatGptApiUrl = "https://api.openai.com/v1/completions";

    internal static async Task<bool> ChatGpt(SocketMessage message)
    {
        // Create a new RestClient instance
        var client = new RestClient(ChatGptApiUrl);

        // Create a new RestRequest instance
        var request = new RestRequest("", Method.Post);

        // Set the request headers
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {ChatGptApiKey}");

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
        string? responseText;
        var success = false;
        // Check the status code of the response
        if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
        {
            // Get the response text from the API
            responseText = JsonConvert.DeserializeObject<dynamic>(response.Content)?["choices"][0]["text"];
            success = true;
        }
        else
        {
            // Get the ErrorMessage from the API
            responseText = response.ErrorMessage;
        }

        // Send the response to the Discord chat
        await message.Channel.SendMessageAsync(responseText);
        return success;
    }
}