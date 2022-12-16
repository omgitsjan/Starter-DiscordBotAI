using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

public class Program
{
    /// <summary>
    ///     This is the Discord token from the bot - (Replace this with your Discord bot token)
    /// </summary>
    private const string DiscordToken = "";

    private DiscordSocketClient _client = null!;
    private InteractionService _commands = null!;

    /// <summary>
    ///     Init
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

        //Creates a config with specified gateway intents
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        // Create a new Discord client
        _client = new DiscordSocketClient(config);
        _commands = new InteractionService(_client);

        // Log messages to the console
        _client.Log += Log;
        _commands.Log += Log;

        // Handle messages received
        _client.MessageReceived += HandleCommand;

        // Login to Discord
        await _client.LoginAsync(TokenType.Bot, DiscordToken);

        // Start the client
        await _client.StartAsync();

        // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        // Block this program until it is closed
        await Task.Delay(-1);
    }


    /// <summary>
    ///     This method is called whenever a message is received
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static async Task HandleCommand(SocketMessage message)
    {
        var success = true;
        var responseText = string.Empty;

        await Log(new LogMessage(LogSeverity.Info, nameof(HandleCommand),
            "New Message incoming..."));

        // Check if the message starts with one of these commands
        switch (message.Content)
        {
            case { } chat when chat.StartsWith("!chat"):
                await Log(new LogMessage(LogSeverity.Info, nameof(HandleCommand),
                    "Recived !chat command: " + message.Content));
                (success, responseText) = await OpenAiService.ChatGpt(message);
                break;
            case { } image when image.StartsWith("!image"):
                await Log(new LogMessage(LogSeverity.Info, nameof(HandleCommand),
                    "Recived !image command: " + message.Content));
                (success, responseText) = await OpenAiService.DallE(message);
                break;
            default:
                await Log(new LogMessage(LogSeverity.Info, nameof(HandleCommand),
                    "No command found, normal message"));
                break;
        }

        if (!success)
            await Log(new LogMessage(LogSeverity.Warning, nameof(HandleCommand),
                "Error with one of the request to the Apis!"));

        if (!string.IsNullOrEmpty(responseText))
            await Log(new LogMessage(LogSeverity.Info, nameof(HandleCommand),
                "Respone: " + responseText));
    }

    /// <summary>
    ///     This method logs messages to the console
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        if (IsDebug())
        {
            // this is where you put the id of the test discord guild
            Console.WriteLine("In debug mode");
            await _commands.RegisterCommandsGloballyAsync();
        }
        else
        {
            // this method will add commands globally, but can take around an hour
            await _commands.RegisterCommandsGloballyAsync();
        }

        Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
    }

    // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
    private static ServiceProvider ConfigureServices()
    {
        // this returns a ServiceProvider that is used later to call for those services
        // we can add types we have access to here, hence adding the new using statement:
        // using csharpi.Services;
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }

    private static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}