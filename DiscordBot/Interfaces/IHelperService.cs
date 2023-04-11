using DiscordBot.Services;
using RestSharp;
using System.Dynamic;

namespace DiscordBot.Interfaces;

public interface IHelperService
{
    public Task<string> GetRandomDeveloperExcuseAsync();
}