using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBot.Services;

public class Watch2GetherService : IWatch2GetherService
{
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

    private readonly IRestClient _httpClient;
    private readonly IConfiguration _configuration;

    public Watch2GetherService(IRestClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
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
    public async Task<Tuple<bool, string>> CreateRoom(string videoUrl)
    {
        // Initialize a new instance of RestClient and empty message and success variables.
        string message;
        var success = false;

        // Retrieve the urls and the apikey from the configuration
        _w2GApiKey = _configuration["Watch2Gether:ApiKey"] ?? string.Empty;
        _w2GCreateRoomUrl = _configuration["Watch2Gether:CreateRoomUrl"] ?? string.Empty;
        _w2GShowRoomUrl = _configuration["Watch2Gether:ShowRoomUrl"] ?? string.Empty;

        if (string.IsNullOrEmpty(_w2GApiKey) || string.IsNullOrEmpty(_w2GCreateRoomUrl) ||
            string.IsNullOrEmpty(_w2GShowRoomUrl))
        {
            message = "Could not load necessary configuration, please provide a valid configuration";
            Program.Log($"{nameof(CreateRoom)}: " + message, LogLevel.Error);
            return new Tuple<bool, string>(success, message);
        }

        // Initialize a new instance of RestRequest with the Watch2Gether room creation URL and HTTP method.
        var request = new RestRequest("", Method.Post);

        // Add required headers and JSON payload to the request.
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddJsonBody(new
        {
            w2g_api_key = _w2GApiKey,
            share = videoUrl
        });

        request.Resource = _w2GCreateRoomUrl;

        // Send the HTTP request asynchronously and await the response.
        var response = await _httpClient.ExecuteAsync(request);

        // If the response content is null, set the error message and return the result Tuple.
        if (string.IsNullOrEmpty(response.Content))
        {
            message = "No response from Watch2Gether";
            Program.Log($"{nameof(CreateRoom)}: " + message, LogLevel.Error);
            return new Tuple<bool, string>(success, message);
        }

        try
        {
            // Deserialize the response content into a dynamic object and extract the streamkey property.
            var responseObj = JsonConvert.DeserializeObject<dynamic>(response.Content);
            message = _w2GShowRoomUrl + responseObj?.streamkey;
            success = true;
        }
        catch (Exception e)
        {
            // Log any deserialization exceptions to the console.
            message = "Failed to deserialize response from Watch2Gether";
            Program.Log($"{nameof(CreateRoom)}: " + message + $" Error: {e.Message}",
                LogLevel.Error);
        }

        if (success)
            Program.Log($"{nameof(CreateRoom)}: Successfully created Watch2Gether room: {message}");
        else
            Program.Log($"{nameof(CreateRoom)}: Failed to create Watch2Gether room. Error: {message}", LogLevel.Error);

        // Return the result Tuple with success status and message.
        return new Tuple<bool, string>(success, message);
    }
}