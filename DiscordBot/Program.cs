using System.Diagnostics;
using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Timer = System.Timers.Timer;

namespace DiscordBot;

public class Program
{
    /// <summary>
    ///     This is the Discord token from the bot - (REPLACE THIS WITH YOUR DISCORD BOT TOKEN)
    /// </summary>
    private const string DiscordToken = "";

    public DiscordClient? Client { get; private set; }

    public static ILogger? Logger { get; private set; }

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
        LogManager.LoadConfiguration("nlog.config");

        // Using decadency injection to configure all services
        await using var services = ConfigureServices();

        Logger = services.GetService<ILogger<Program>>();

        if (string.IsNullOrEmpty(DiscordToken))
        {
            Log("Discord token is empty. Please provide a valid token and restart the bot!", LogLevel.Error);
            Environment.Exit(404);
            return;
        }

        // Create a new Discord client with specified gateway intents
        var config = new DiscordConfiguration
        {
            Token = DiscordToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages,
            LoggerFactory = services.GetService<ILoggerFactory>()
        };

        // Creating the Discord Bot Client
        Client = new DiscordClient(config);

        // Configured the Slash Commands
        var slashCommandsConfig = Client.UseSlashCommands();
        slashCommandsConfig.RegisterCommands<SlashCommands>();


        Client.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(1)
        });

        // Log messages to the console
        Log("omgitsjan/DiscordBot is running!");

        if (IsDebug()) Log("Debug Mode...", LogLevel.Debug);

        // Connect to Discord
        await Client.ConnectAsync();

        // Set up the timer to change the status every 30 seconds
        var statusIndex = 0; // This variable helps us cycle through the different statuses
        var timer = new Timer(15000); // Set the timer to execute every 15 seconds

        timer.Elapsed += async (sender, e) =>
        {
            switch (statusIndex)
            {
                case 0:
                    var currentBitcoinPrice = await GetCurrentBitcoinPriceAsync();
                    var activity1 = new DiscordActivity($"BTC: ${currentBitcoinPrice}", ActivityType.Watching);
                    await Client.UpdateStatusAsync(activity1);
                    break;
                case 1:
                    var currentDate = DateTime.UtcNow.ToString("dd.MM.yyyy");
                    await Client.UpdateStatusAsync(new DiscordActivity($"Date: {currentDate}",
                        ActivityType.Watching));
                    break;
                case 2:
                    var currentDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
                    await Client.UpdateStatusAsync(new DiscordActivity($"Time: {currentDateTime} UTC",
                        ActivityType.Watching));
                    break;
                case 3:
                    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    var uptimeString = $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
                    await Client.UpdateStatusAsync(
                        new DiscordActivity($"Uptime: {uptimeString}", ActivityType.Watching));
                    break;
                case 4:
                    var developerExcuse = await GetRandomDeveloperExcuseAsync();
                    await Client.UpdateStatusAsync(new DiscordActivity($"Excuse: {developerExcuse[..110]}",
                        ActivityType.ListeningTo));
                    break;
            }

            statusIndex = (statusIndex + 1) % 5; // Cycle through the status options
        };

        timer.AutoReset = true;
        timer.Enabled = true;

        // Block this program until it is closed
        await Task.Delay(-1);
    }


    /// <summary>
    ///     Gets the current Bitcion price from bybit public api
    /// </summary>
    /// <returns>The current price from bitcoin as BTCUSD string</returns>
    private static async Task<string> GetCurrentBitcoinPriceAsync()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://api.bybit.com/v2/public/tickers?symbol=BTCUSD");
        var jsonString = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonString);
        return json["result"]?[0]?["last_price"]?.Value<string>() ?? "Could not fetch current Bitcoin price...";
    }

    /// <summary>
    ///     Gets a random dev excuse from the open dev-excuses-api (herokuapp.com)
    /// </summary>
    /// <returns></returns>
    private static async Task<string> GetRandomDeveloperExcuseAsync()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://dev-excuses-api.herokuapp.com/");
        var excuse = await response.Content.ReadAsStringAsync();
        return excuse.Trim('"');
    }

    /// <summary>
    ///     This method logs messages to the console
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    internal static void Log(string msg,
        LogLevel logLevel = LogLevel.Information)
    {
        if (Logger != null)
            Logger.Log(logLevel, msg);
        else
            Console.WriteLine(msg);
    }

    /// <summary>
    ///     This method handles the ServiceCollection creation/configuration, and builds out the service provider we can call
    ///     on later
    /// </summary>
    /// <returns></returns>
    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<SlashCommands>()
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