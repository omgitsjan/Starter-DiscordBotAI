using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Wrapper
{
    public class InteractionContextWrapper(BaseContext context) : IInteractionContextWrapper
    {
        private DiscordChannel? _discordChannel;
        private DiscordUser? _discordUser;

        public void SetUpForTesting(DiscordChannel? discordChannel, DiscordUser? discordUser)
        {
            _discordChannel = discordChannel;
            _discordUser = discordUser;
        }

        public DiscordChannel Channel => _discordChannel ?? context.Channel;

        public DiscordUser User => _discordUser ?? context.User;

        public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
        {
            return context.CreateResponseAsync(type, builder);
        }

        public Task DeleteResponseAsync()
        {
            return context.DeleteResponseAsync();
        }

        public Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed? embed = null)
        {
            return context.Channel.SendMessageAsync(content, embed);
        }
    }
}