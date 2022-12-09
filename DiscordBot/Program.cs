using Discord;
using Discord.WebSocket;
using OpenAI_Discord_Bot.Service;

namespace OpenAI_Discord_Bot;

public class Program
{
    /// <summary>
    ///     This is the Discord token from the bot - (Replace this with your Discord bot token)
    /// </summary>
    private const string DiscordToken = "";

    /// <summary>
    ///     Init
    /// </summary>
    private static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Main Program
    /// </summary>
    /// <returns></returns>
    public static async Task MainAsync()
    {
        //Creates a config with specified gateway intents
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        // Create a new Discord client
        var client = new DiscordSocketClient(config);

        // Log messages to the console
        client.Log += Log;

        // Handle messages received
        client.MessageReceived += HandleCommand;

        // Login to Discord
        await client.LoginAsync(TokenType.Bot, DiscordToken);

        // Start the client
        await client.StartAsync();

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
        // Check if the message starts with the !chat command
        if (message.Content.StartsWith("!chat")) await OpenAiService.ChatGpt(message);
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
}