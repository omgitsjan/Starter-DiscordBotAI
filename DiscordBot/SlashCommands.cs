using DiscordBot.Interfaces;
using DiscordBot.Wrapper;
using DSharpPlus.SlashCommands;

namespace DiscordBot
{
    public class SlashCommands : ApplicationCommandModule
    {
        private readonly ISlashCommandsService _slashCommandsService;

        public SlashCommands(ISlashCommandsService slashCommandsService)
        {
            _slashCommandsService = slashCommandsService;
        }

        [SlashCommand("ping",
            "This is a basic ping command to check if the Bot is online and what the current Latency is")]
        public async Task PingSlashCommand(InteractionContext ctx)
        {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.PingSlashCommandAsync(context);
        }

        [SlashCommand("ChatGPT",
            "Send a custom Text to the OpenAI - ChatGPT API and get a response from their AI based on your input")]
        public async Task ChatSlashCommand(InteractionContext ctx,
            [Option("prompt", "Write an input that the ChatGPT AI should respond to")]
            string text)
        {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.ChatSlashCommandAsync(context, text);
        }

        [SlashCommand("DALL-E",
            "Send a custom Text to the OpenAI - DALL-E Api and get a generated Image based on input")]
        public async Task ImageSlashCommand(InteractionContext ctx,
            [Option("prompt", "Write a Text on how the generated Image should look like")]
            string text)
        {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.ImageSlashCommandAsync(context, text);
        }

        [SlashCommand("Watch2Gether",
            "Creates a room for you and your friends in Watch2Gether")]
        public async Task Watch2GetherSlashCommand(InteractionContext ctx,
            [Option("Video-URL", "Insert a Video-URL that should auto start after creating a Watch2Gether Room")]
            string url = "")
        {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.Watch2GetherSlashCommandAsync(context, url);
        }

        [SlashCommand("Weather", "Get the current weather for the specified city")]
        public async Task WeatherSlashCommand(InteractionContext ctx,
            [Option("city", "The city you want to get the weather for")]
            string city)
        {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.WeatherSlashCommandAsync(context, city);
        }

        [SlashCommand("crypto",
            "Gets the price for a given Cryptocurrency")]
        public async Task CryptoSlashCommand(InteractionContext ctx,
            [Option("Symbol", "The Cryptocurrency you want to get the price for")]
            string symbol = "") {
            InteractionContextWrapper context = new InteractionContextWrapper(ctx);
            await _slashCommandsService.CryptoSlashCommandAsync(context, symbol);
        }
    }
}