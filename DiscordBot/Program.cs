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

namespace DiscordBot;

public class Program
{
    /// <summary>
    ///     This is the Discord token from the bot - (REPLACE THIS WITH YOUR DISCORD BOT TOKEN)
    /// </summary>
    private const string DiscordToken = "";

    public DiscordClient? Client { get; private set; }

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
        await using var services = ConfigureServices();

        // Create a new Discord client with specified gateway intents
        var config = new DiscordConfiguration
        {
            Token = DiscordToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages
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
        Log("omgitsjan/DiscordBot is running!", Client);

        // Connect to Discord
        await Client.ConnectAsync();

        // Set up the timer to change the status every 30 seconds
        var statusIndex = 0; // This variable helps us cycle through the different statuses
        var timer = new Timer(async _ =>
        {
            switch (statusIndex)
            {
                case 0:
                    var currentBitcoinPrice = await GetCurrentBitcoinPriceAsync();
                    var activity1 = new DiscordActivity($"BTC: ${currentBitcoinPrice}", ActivityType.Watching);
                    await Client.UpdateStatusAsync(activity1);
                    break;
                case 1:
                    var currentDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
                    var activity2 = new DiscordActivity($"Time: {currentDateTime} UTC", ActivityType.Watching);
                    await Client.UpdateStatusAsync(activity2);
                    break;
                case 2:
                    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    var uptimeString = $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
                    var activity3 = new DiscordActivity($"Uptime: {uptimeString}", ActivityType.Watching);
                    await Client.UpdateStatusAsync(activity3);
                    break;
                case 3:
                    var developerExcuse = await GetRandomDeveloperExcuseAsync();
                    var activity4 = new DiscordActivity($"Excuse: {developerExcuse}", ActivityType.Custom);
                    await Client.UpdateStatusAsync(activity4);
                    break;
            }

            statusIndex = (statusIndex + 1) % 4; // Cycle through the status options
        }, null, (long)0, 15000); // Set the timer to execute every 15 seconds

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
    /// <param name="client"></param>
    /// <returns></returns>
    internal static void Log(string msg, BaseDiscordClient? client = null)
    {
        if (client != null)
            client.Logger.LogInformation(msg);
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
            .BuildServiceProvider();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}