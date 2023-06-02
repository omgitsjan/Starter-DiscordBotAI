using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordBot.Wrapper
{
    public interface IInteractionContextWrapper
    {
        DiscordChannel Channel { get; }
        DiscordUser User { get; }
        Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null);
        Task DeleteResponseAsync();
        public void SetUpForTesting(DiscordChannel? channel, DiscordUser? user);
    }
}