using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Services;

internal class SlashCommands : ApplicationCommandModule
{
    [SlashCommand("ping",
        "This is a basic ping command to check if the Bot is online and what the current Latency is")]
    public async Task PingSlashCommand(InteractionContext ctx)
    {
        // Creating the ping to measure response time
        var pinger = new Ping();

        // Creating a Message in the channel
        await ctx.Channel.SendMessageAsync("Ping...");

        // Starts the Response with a thinking state
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Sending Ping...!"));

        // Stop the stopwatch and output the elapsed time
        var reply = pinger.Send("google.com");

        // Creates the Message that should display
        var embedMessage = new DiscordEmbedBuilder
        {
            Title = "Pong!",
            Description = $"Latency is: {reply.RoundtripTime} ms",
            Url = "https://github.com/omgitsjan/DiscordBotAI",
            Timestamp = DateTimeOffset.UtcNow,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "omgitsjan/DiscordBot",
                IconUrl = "https://avatars.githubusercontent.com/u/42674570?v=4"
            }
        };

        // Sending the Embed Message to the Channel
        await ctx.Channel.SendMessageAsync(embedMessage);

        // Deleting the thinking state
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("ChatGPT",
        "Send a custom Text to the OpenAI - ChatGPT API and get a response from their AI based on your input")]
    public async Task ChatSlashCommand(InteractionContext ctx,
        [Option("prompt", "Write an input that the ChatGPT AI should respond to")]
        string text)
    {
        // Creating a Message in the channel
        await ctx.Channel.SendMessageAsync("Request from " + ctx.User.Mention + ": " +
                                           text);

        // Starts the Response with a thinking state
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Sending Request to ChatGPT API..."));

        // Execute and waiting for the response from our Method
        var (success, message) = await OpenAiService.ChatGpt(text);

        // Creating embed Message via DiscordEmbedBuilder
        var embedMessage = new DiscordEmbedBuilder
        {
            Title = "ChatGPT",
            Description = message,
            Timestamp = DateTimeOffset.UtcNow,
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = ctx.User.Username,
                IconUrl = ctx.User.AvatarUrl
            },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Powered by OpenAI",
                IconUrl =
                    "https://seeklogo.com/images/O/open-ai-logo-8B9BFEDC26-seeklogo.com.png"
            }
        };

        // Log if anything goes wrong while executing the request
        if (!success) Program.Log(message);

        // Sending the Embed Message to the Channel
        await ctx.Channel.SendMessageAsync(embedMessage);

        // Deleting the thinking state
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("DALL-E",
        "Send a custom Text to the OpenAI - DALL-E Api and get a generated Image based on input")]
    public async Task ImageSlashCommand(InteractionContext ctx,
        [Option("prompt", "Write a Text on how the generated Image should look like")]
        string text)
    {
        // Send a message indicating that the command is being executed
        await ctx.Channel.SendMessageAsync("Request from " + ctx.User.Mention + " : " +
                                           text);

        // Send a "thinking" response to let the user know that the bot is working on their request
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Sending Request to DALL-E API..."));

        // Execute the DALL-E API request and wait for a response
        var (sucess, message) = await OpenAiService.DallE(text);

        // Extract the image URL from the response message using a regular expression
        var url = Regex.Match(message, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?").ToString();

        // Create an embed message to display the generated image
        var embedMessage = new DiscordEmbedBuilder
        {
            Title = "DALL-E",
            Description = message,
            ImageUrl = url,
            Timestamp = DateTimeOffset.UtcNow,
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = ctx.User.Username,
                IconUrl = ctx.User.AvatarUrl
            },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Powered by OpenAI",
                IconUrl =
                    "https://seeklogo.com/images/O/open-ai-logo-8B9BFEDC26-seeklogo.com.png"
            }
        };

        // If the API request was not successful, log the error message
        if (!sucess) Program.Log(message);

        // Send the embed message with the generated image to the channel
        await ctx.Channel.SendMessageAsync(embedMessage);

        // Deleting the thinking state
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("Watch2Gether",
        "Creates a room for you and your friends in Watch2Gether")]
    public async Task Watch2GetherSlashCommand(InteractionContext ctx,
        [Option("Video-URL", "Insert a Video-URL that should auto start after creating a Watch2Gether Room")]
        string url = "")
    {
        // Send a message indicating that the command is being executed
        await ctx.Channel.SendMessageAsync("Creating a Watch2Gether Room for " + ctx.User.Mention);

        // Send a "thinking" response to let the user know that the bot is working on their request
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Sending create Room request to Watch2Gether API..."));

        // Call CreateRoom in the Watch2GetherService to send a create room request to the Watch2Gether Api
        var (sucess, message) = await Watch2GetherService.CreateRoom(url);

        // Creates the Message that should display
        var embedMessage = new DiscordEmbedBuilder
        {
            Title = "Watch2Gether Room!",
            Description = $"This Room was created for you and is available under the following link: {message}",
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = ctx.User.Username,
                IconUrl = ctx.User.AvatarUrl
            },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Watch2Gether",
                IconUrl = "https://w2g.tv/assets/256.f5817612.png"
            },
            Timestamp = DateTimeOffset.UtcNow
        };

        // If the API request was not successful, log the error message
        if (!sucess)
        {
            Program.Log(message);
            embedMessage.Description = message;
        }


        // Sending the Embed Message to the Channel
        await ctx.Channel.SendMessageAsync(embedMessage);

        // Deleting the thinking state
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("Weather", "Get the current weather for the specified city")]
    public async Task WeatherSlashCommand(InteractionContext ctx,
        [Option("city", "The city you want to get the weather for")]
        string city)
    {
        // Send a "thinking" response to let the user know that the bot is working on their request
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Fetching weather data..."));

        // Call GetWeatherAsync to fetch the weather data for the specified city
        var (success, message, weather) = await OpenWeatherMapService.GetWeatherAsync(city);

        if (success)
        {
            var embedMessage = new DiscordEmbedBuilder
            {
                Title = $"Weather in {city} - {weather?.Temperature:F2}°C",
                Description = message,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.User.Username,
                    IconUrl = ctx.User.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Weather data provided by OpenWeatherMap",
                    IconUrl = "https://openweathermap.org/themes/openweathermap/assets/img/logo_white_cropped.png"
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embedMessage);
        }
        else
        {
            await ctx.Channel.SendMessageAsync(message);
        }

        // Deleting the thinking state
        await ctx.DeleteResponseAsync();
    }
}