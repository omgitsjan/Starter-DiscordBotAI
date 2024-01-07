using DiscordBot.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services
{
    public class HelperService : IHelperService
    {
        private readonly List<string> _developerExcuses = [];
        private readonly ILogger<HelperService> _logger;

        public HelperService(ILogger<HelperService> logger)
        {
            _logger = logger;
            LoadDeveloperExcuses(null);
        }

        public HelperService(ILogger<HelperService> logger, string? excusesFilePath)
        {
            _logger = logger;
            LoadDeveloperExcuses(excusesFilePath);
        }

        private void LoadDeveloperExcuses(string? excusesFilePath)
        {
            string filePath = excusesFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Data", "excuses.json");

            if (!File.Exists(filePath))
            {
                _logger.LogError("Developer excuses file not found at: {FilePath}", filePath);
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                JObject json = JObject.Parse(jsonContent);
                _developerExcuses.AddRange(json["en"]?.ToObject<List<string>>() ?? throw new Exception());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading developer excuses from file: {FilePath}", filePath);
            }
        }

        public Task<string> GetRandomDeveloperExcuseAsync()
        {
            if (_developerExcuses.Count == 0)
            {
                return Task.FromResult("Could not fetch a Developer excuse. Please check the configuration.");
            }

            Random random = new();
            int index = random.Next(_developerExcuses.Count);
            return Task.FromResult(_developerExcuses[index]);
        }
    }
}