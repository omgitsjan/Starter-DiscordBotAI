using DiscordBot.Services;
using RestSharp;
using System.Dynamic;

namespace DiscordBot.Interfaces;

public interface IHelperService
{
    public Task<string> GetRandomDeveloperExcuseAsync();

    public Task<HttpResponse> GetResponseFromURL(string resource, Method method = Method.Get, string? errorMessage = null, List<KeyValuePair<string, string>> headers = null, string jsonBodyString = null);
}