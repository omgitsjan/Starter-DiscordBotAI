using DiscordBot.Interfaces;
using Microsoft.Extensions.Logging;
using RestSharp;
using Newtonsoft.Json;

namespace DiscordBot.Services;

public class HttpService : IHttpService
{
    private readonly IRestClient _httpClient;

    public HttpService(IRestClient httpClient)
    {
        _httpClient = httpClient;
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
