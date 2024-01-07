using System.Diagnostics;
using DiscordBot.Interfaces;
using DiscordBot.Services;
using DiscordBot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using RestSharp;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Timer = System.Timers.Timer;

namespace DiscordBot
{
    public class Program
    {
        /// <summary>
        ///     This is the Discord token from the bot
        /// </summary>
        private string? _discordToken;

        /// <summary>
        ///     The DiscordClient instance used for connecting to and interacting with Discord
        /// </summary>
        public DiscordClient? Client { get; private set; }

        /// <summary>
        ///     The ILogger instance used for logging messages
        /// </summary>
        public static ILogger? Logger { get; private set; }

        /// <summary>
        ///     The IHelperService instance used for various helper functions
        /// </summary>
        public static IHelperService? HelperService { get; private set; }

        public static ICryptoService? CryptoService { get; private set; }

        /// <summary>
        ///     Init Program
        /// </summary>
        public static Task Main(string[] args)
        {
            return new Program().MainAsync();
        }

        /// <summary>
        ///     Main Program
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            // Configure NLog
            LogManager.Setup().LoadConfigurationFromFile("nlog.config");

            // Load the configuration from the appsettings.json file
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Using decadency injection to configure all services
            await using ServiceProvider services = ConfigureServices(configuration);

            Logger = services.GetService<ILogger<Program>>();
            HelperService = services.GetService<IHelperService>();
            CryptoService = services.GetService<ICryptoService>();

            if (HelperService == null || Logger == null || CryptoService == null)
            {
                Log(
                    "Not all Services could be loaded. Please check the code or open a Issue on Github via omgitsjan/DiscordBotAI!",
                    LogLevel.Critical);
                Environment.Exit(500);
                return;
            }

            // Retrieve the Discord token from the configuration
            _discordToken = configuration["DiscordBot:Token"] ?? string.Empty;

            if (string.IsNullOrEmpty(_discordToken))
            {
                Log("Could not load Discord token. Please check if a valid token was provided and restart the bot!",
                    LogLevel.Error);
                Environment.Exit(404);
                return;
            }
            Log("Got Token: " + _discordToken);

            // Create a new Discord client with specified gateway intents
            DiscordConfiguration config = new()
            {
                Token = _discordToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages,
                LoggerFactory = services.GetService<ILoggerFactory>()
            };

            // Creating the Discord Bot Client
            Client = new DiscordClient(config);

            // Configured the Slash Commands
            SlashCommandsExtension? slashCommandsConfig = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });
            slashCommandsConfig.RegisterCommands<SlashCommands>();


            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            // Log messages to the console
            Log("omgitsjan/DiscordBot is running!");

            if (IsDebug())
            {
                Log("Debug Mode...", LogLevel.Debug);
            }

            // Connect to Discord
            await Client.ConnectAsync();

            // Set up the timer to change the status every 30 seconds
            int statusIndex = 0; // This variable helps us cycle through the different statuses
            Timer timer = new(15000); // Set the timer to execute every 30 seconds

            timer.Elapsed += async (_, _) =>
            {
                switch (statusIndex)
                {
                    case 0:
                        (bool success, string? currentBitcoinPrice) = await CryptoService.GetCryptoPriceAsync("BTC", "USDT");
                        DiscordActivity activity1 =
                            new(
                                $"BTC: ${(currentBitcoinPrice.Length > 110 ? currentBitcoinPrice[..110] : currentBitcoinPrice)}",
                                ActivityType.Watching);
                        if (!success)
                        {
                            activity1.Name = "Failed to fetch BTC Price...";
                        }

                        await Client.UpdateStatusAsync(activity1);
                        break;
                    case 1:
                        string currentDate = DateTime.UtcNow.ToString("dd.MM.yyyy");
                        await Client.UpdateStatusAsync(new DiscordActivity($"Date: {currentDate}",
                            ActivityType.Watching));
                        break;
                    case 2:
                        string currentDateTime = DateTime.UtcNow.ToString("HH:mm");
                        await Client.UpdateStatusAsync(new DiscordActivity($"Time: {currentDateTime} UTC",
                            ActivityType.Watching));
                        break;
                    case 3:
                        TimeSpan uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                        string uptimeString = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                        await Client.UpdateStatusAsync(
                            new DiscordActivity($"Uptime: {uptimeString}", ActivityType.Watching));
                        break;
                    case 4:
                        int memberCount = Client.Guilds.Sum(g => g.Value.MemberCount);
                        await Client.UpdateStatusAsync(new DiscordActivity(
                            $"Available to '{memberCount}' Users", ActivityType.Watching));
                        break;
                    case 5:
                        string? developerExcuse = await HelperService.GetRandomDeveloperExcuseAsync();
                        await Client.UpdateStatusAsync(new DiscordActivity(
                            $"Excuse: {(developerExcuse.Length > 110 ? developerExcuse[..110] : developerExcuse)}",
                            ActivityType.ListeningTo));
                        break;
                    case 6:
                        await Client.UpdateStatusAsync(new DiscordActivity(
                            "omgitsjan/DiscordBotAI | janpetry.de",
                            ActivityType.Watching));
                        break;
                }

                statusIndex = (statusIndex + 1) % 7; // Cycle through the status options
            };

            timer.AutoReset = true;
            timer.Enabled = true;

            // Block this program until it is closed
            await Task.Delay(-1);
        }

        /// <summary>
        ///     This method logs messages to the console
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        internal static void Log(string? msg,
            LogLevel logLevel = LogLevel.Information)
        {
            if (Logger != null)
            {
                Logger.Log(logLevel, "{Message}", msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        /// <summary>
        ///     This method handles the ServiceCollection creation/configuration, and builds out the service provider we can call
        ///     on later
        /// </summary>
        /// <returns></returns>
        private static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton<IHttpService, HttpService>()
                .AddSingleton<IWatch2GetherService, Watch2GetherService>()
                .AddSingleton<IOpenWeatherMapService, OpenWeatherMapService>()
                .AddSingleton<IOpenAiService, OpenAiService>()
                .AddSingleton<ICryptoService, CryptoService>()
                .AddSingleton<IHelperService, HelperService>()
                .AddSingleton<IInteractionContextWrapper, InteractionContextWrapper>()
                .AddSingleton<ISlashCommandsService, SlashCommandsService>()
                .AddSingleton<SlashCommands>()
                .AddSingleton<IRestClient>(_ => new RestClient())
                .AddLogging(loggingBuilder => loggingBuilder.AddNLog())
                .BuildServiceProvider();
        }

        /// <summary>
        ///     Check if the programm is running in Debug mode
        /// </summary>
        /// <returns>Boolean that indicates if the bot is running in Debug mode</returns>
        private static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}