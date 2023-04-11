using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services;

public class HelperService : IHelperService
{
    private string? _developerExcuseApiUrl;

    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;

    public HelperService(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
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

        var response = await _httpService.GetResponseFromURL(_developerExcuseApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            return response.Content;
        }

        var json = JObject.Parse(response.Content ?? "{}");

        return json["text"]?.Value<string>() ?? "Could not fetch current Developer excuse...";
    }
}
