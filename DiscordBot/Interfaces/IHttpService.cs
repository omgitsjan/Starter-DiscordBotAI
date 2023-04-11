using DiscordBot.Services;
using RestSharp;

namespace DiscordBot.Interfaces;

public interface IHttpService
{
    public Task<HttpResponse> GetResponseFromURL(string resource, Method method = Method.Get, string? errorMessage = null, List<KeyValuePair<string, string>> headers = null, string jsonBodyString = null);
}