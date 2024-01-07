using System.Net;
using DiscordBot.Interfaces;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DiscordBot.Services
{
    public class HttpService(IRestClient httpClient) : IHttpService
    {
        /// <summary>
        ///     Gets the response from a URL and handles errors
        /// </summary>
        /// <returns>The current price from bitcoin as BTC-USDT string</returns>
        public async Task<HttpResponse> GetResponseFromUrl(string resource, Method method = Method.Get,
            string? errorMessage = null, List<KeyValuePair<string, string>>? headers = null, object? jsonBody = null)
        {
            RestRequest request = new(resource, method);

            if (headers != null && headers.Count != 0)
            {
                headers.ForEach(header => request.AddHeader(header.Key, header.Value));
            }

            if (jsonBody != null)
            {
                request.AddJsonBody(jsonBody);
            }

            RestResponse response = new();

            // Send the HTTP request asynchronously and await the response.
            try
            {
                response = await httpClient.ExecuteAsync(request);
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

    public class HttpResponse(bool isSuccessStatusCode, string? content)
    {
        public bool IsSuccessStatusCode { get; set; } = isSuccessStatusCode;
        public string? Content { get; set; } = content;
    }
}