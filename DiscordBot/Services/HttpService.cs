using System.Net;
using DiscordBot.Interfaces;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DiscordBot.Services
{
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
            string? errorMessage = null, List<KeyValuePair<string, string>>? headers = null, object? jsonBody = null)
        {
            RestRequest request = new RestRequest(resource, method);

            if (headers != null && headers.Any())
            {
                headers.ForEach(header => request.AddHeader(header.Key, header.Value));
            }

            if (jsonBody != null)
            {
                request.AddJsonBody(jsonBody);
            }

            RestResponse response = new RestResponse();

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
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            string? content = response.Content;

            if (response.IsSuccessStatusCode)
            {
                return new HttpResponse(response.IsSuccessStatusCode, content);
            }

            content = $"StatusCode: {response.StatusCode} | {errorMessage ?? response.ErrorMessage}";
            Program.Log(content, LogLevel.Error);

            return new HttpResponse(response.IsSuccessStatusCode, content);
        }
    }

    public class HttpResponse
    {
        public HttpResponse(bool isSuccessStatusCode, string? content)
        {
            IsSuccessStatusCode = isSuccessStatusCode;
            Content = content;
        }

        public bool IsSuccessStatusCode { get; set; }
        public string? Content { get; set; }
    }
}