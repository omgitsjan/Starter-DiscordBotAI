using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using Newtonsoft.Json;

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

        var response = await GetResponseFromURL(_developerExcuseApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            return response.Content;
        }

        var json = JObject.Parse(response.Content ?? "{}");

        return json["text"]?.Value<string>() ?? "Could not fetch current Developer excuse...";
    }

    /// <summary>
    ///     Gets the response from an URL and handles errors
    /// </summary>
    /// <returns>The current price from bitcoin as BTCUSD string</returns>
    public async Task<HttpResponse> GetResponseFromURL(string resource, Method method = Method.Get, string? errorMessage = null, List<KeyValuePair<string, string>> headers = null, string jsonBodyString = null)
    {
        var request = new RestRequest(resource, method);

        if (headers != null && headers.Any())
        {
            headers.ForEach(header => request.AddHeader(header.Key, header.Value));
        }

        if (!String.IsNullOrEmpty(jsonBodyString))
        {
            request.AddJsonBody(JsonConvert.DeserializeObject<object>(jsonBodyString));
        }

        // Send the HTTP request asynchronously and await the response.
        var response = await _httpClient.ExecuteAsync(request);
        var content = response.Content;

        if (!response.IsSuccessStatusCode)
        {
            content = $"StatusCode: {response.StatusCode} | {errorMessage ?? response.ErrorMessage}";
            Program.Log(content, LogLevel.Error);
        }

        return new HttpResponse(response.IsSuccessStatusCode, content);
    }
}

public class HttpResponse
{
    public bool IsSuccessStatusCode { get; set; }
    public string Content { get; set; }

    public HttpResponse(bool IsSuccessStatusCode, string Content)
    {
        this.IsSuccessStatusCode = IsSuccessStatusCode;
        this.Content = Content;
    }
}
