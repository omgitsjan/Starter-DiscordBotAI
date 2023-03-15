using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        // Block this program until it is closed
        await Task.Delay(-1);
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