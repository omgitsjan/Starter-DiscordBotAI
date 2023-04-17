using DiscordBot.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

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
    public async Task<HttpResponse> GetResponseFromUrl(string resource, Method method = Method.Get,
        string? errorMessage = null, List<KeyValuePair<string, string>>? headers = null, string jsonBodyString = "")
    {
        var request = new RestRequest(resource, method);

        if (headers != null && headers.Any())
        {
            headers.ForEach(header => request.AddHeader(header.Key, header.Value));
        }

        if (!string.IsNullOrEmpty(jsonBodyString))
        {
            request.AddJsonBody(JsonConvert.DeserializeObject(jsonBodyString) ?? "");
        }

        var response = new RestResponse();

        // Send the HTTP request asynchronously and await the response.
        try
        {
            response = await _httpClient.ExecuteAsync(request);
        }
        catch (Exception e)
        {
            response.IsSuccessStatusCode = false;
            response.ErrorMessage = $"({nameof(GetResponseFromUrl)}): Unknown error occurred" + e.Message;
            response.ErrorException = e;
            response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
        }

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
    public string? Content { get; set; }

    public HttpResponse(bool isSuccessStatusCode, string? content)
    {
        IsSuccessStatusCode = isSuccessStatusCode;
        Content = content;
    }
}
