using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DiscordBot.Interfaces;
using DiscordBot.Models;
using DiscordBot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordBot.Services
{
    public class SlashCommandsService : ISlashCommandsService
    {
        private readonly IOpenAiService _openAiService;
        private readonly IOpenWeatherMapService _openWeatherMapService;
        private readonly IWatch2GetherService _watch2GetherService;
        private readonly ICryptoService _cryptoService;

        public SlashCommandsService(IWatch2GetherService watch2GetherService,
            IOpenWeatherMapService openWeatherMapService,
            IOpenAiService openAiService,
            ICryptoService cryptoService)
        {
            _watch2GetherService = watch2GetherService;
            _openWeatherMapService = openWeatherMapService;
            _openAiService = openAiService;
            _cryptoService = cryptoService;
        }

        public async Task PingSlashCommandAsync(IInteractionContextWrapper ctx)
        {
            // Creating the ping to measure response time
            Ping pinger = new();

            // Creating a Message in the channel
            await ctx.Channel.SendMessageAsync("Ping...");

            // Starts the Response with a thinking state
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Sending Ping...!"));

            // Stop the stopwatch and output the elapsed time
            PingReply reply = pinger.Send("google.com");

            // Creates the Message that should display
            DiscordEmbedBuilder embedMessage = new()
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

            // Logging the success of the command with message and user details
            Program.Log(
                $"Command '{nameof(PingSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}).");
        }

        public async Task ChatSlashCommandAsync(IInteractionContextWrapper ctx, string text)
        {
            // Creating a Message in the channel
            await ctx.Channel.SendMessageAsync("Request from " + ctx.User.Mention + ": " +
                                               text);

            // Starts the Response with a thinking state
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Sending Request to ChatGPT API..."));

            // Execute and waiting for the response from our Method
            (bool success, string? message) = await openAiService.ChatGptAsync(text);

            // Creating embed Message via DiscordEmbedBuilder
            DiscordEmbedBuilder embedMessage = new()
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
            if (!success)
            {
                Program.Log(message);
            }

            // Sending the Embed Message to the Channel
            await ctx.Channel.SendMessageAsync(embedMessage);

            // Deleting the thinking state
            await ctx.DeleteResponseAsync();

            // Logging the success of the command with message and user details
            Program.Log(
                $"Command '{nameof(ChatSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}). Input text: {text}");
        }

        public async Task ImageSlashCommandAsync(IInteractionContextWrapper ctx, string text)
        {
            // Send a message indicating that the command is being executed
            await ctx.Channel.SendMessageAsync("Request from " + ctx.User.Mention + " : " +
                                               text);

            // Send a "thinking" response to let the user know that the bot is working on their request
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Sending Request to DALL-E API..."));

            // Execute the DALL-E API request and wait for a response
            (bool sucess, string message) = await openAiService.DallEAsync(text);

            // Extract the image URL from the response message using a regular expression
            string url = Regex.Match(message, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?").ToString();

            // Create an embed message to display the generated image
            DiscordEmbedBuilder embedMessage = new()
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
            if (!sucess)
            {
                Program.Log(message);
            }

            // Send the embed message with the generated image to the channel
            await ctx.Channel.SendMessageAsync(embedMessage);

            // Deleting the thinking state
            await ctx.DeleteResponseAsync();

            // Logging the success of the command with message and user details
            Program.Log(
                $"Command '{nameof(ImageSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}). Input text: {text}");
        }

        public async Task Watch2GetherSlashCommandAsync(IInteractionContextWrapper ctx, string url)
        {
            // Send a message indicating that the command is being executed
            await ctx.Channel.SendMessageAsync("Creating a Watch2Gether Room for " + ctx.User.Mention);

            // Send a "thinking" response to let the user know that the bot is working on their request
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(
                    "Sending create Room request to Watch2Gether API..."));

            // Call CreateRoom in the Watch2GetherService to send a create room request to the Watch2Gether Api
            (bool success, string? message) = await watch2GetherService.CreateRoom(url);

            // Creates the Message that should display
            DiscordEmbedBuilder embedMessage = new()
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
            if (!success)
            {
                Program.Log(message);
                embedMessage.Description = message;
                Program.Log(
                    $"Command '{nameof(Watch2GetherSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}).");
            }


            // Sending the Embed Message to the Channel
            await ctx.Channel.SendMessageAsync(embedMessage);

            // Deleting the thinking state
            await ctx.DeleteResponseAsync();

            // Logging the success of the command with message and user details
            if (success)
            {
                Program.Log(
                    $"Command '{nameof(Watch2GetherSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}).");
            }
        }

        public async Task WeatherSlashCommandAsync(IInteractionContextWrapper ctx, string city)
        {
            // Send a "thinking" response to let the user know that the bot is working on their request
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Fetching weather data..."));

            // Call GetWeatherAsync to fetch the weather data for the specified city
            (bool success, string message, WeatherData? weather) = await openWeatherMapService.GetWeatherAsync(city);

            if (success)
            {
                DiscordEmbedBuilder embedMessage = new()
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

            // Logging the success of the command with message and user details
            if (success)
            {
                Program.Log(
                    $"Command '{nameof(WeatherSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}). City: {city}");
            }
        }       

        public async Task CryptoSlashCommandAsync(IInteractionContextWrapper ctx, string symbol = "BTC", string physicalCurrency = "USDT") {
            symbol = symbol.ToUpper();

            // Send a "thinking" response to let the user know that the bot is working on their request
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(
                    "Requesting " + symbol + " from ByBit API..."));

            // Call GetCryptoPriceAsync to get the current Price
            (bool success, string? message) = await _cryptoService.GetCryptoPriceAsync(symbol, physicalCurrency);
            Program.Log("Message: " + message);

            if (success) {
                DiscordEmbedBuilder embedMessage = new DiscordEmbedBuilder
                {
                    Title = $"{symbol} - {physicalCurrency} | ${message}",
                    Description = $"Price of {symbol} - {physicalCurrency} is ${message}",
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.User.Username,
                        IconUrl = ctx.User.AvatarUrl
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "Data provided by ByBit",
                        IconUrl = "https://seeklogo.com/images/B/bybit-logo-4C31FD6A08-seeklogo.com.png"
                    },
                    Timestamp = DateTimeOffset.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embedMessage);
            } else {
                await ctx.Channel.SendMessageAsync(message);
            }

            // Deleting the thinking state
            await ctx.DeleteResponseAsync();

            // Logging the success of the command with message and user details
            if (success) {
                Program.Log(
                    $"Command '{nameof(CryptoSlashCommandAsync)}' executed successfully by user {ctx.User.Username} ({ctx.User.Id}). Symbol: {symbol}");
            }
        }

    }
}