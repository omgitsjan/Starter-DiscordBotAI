using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBot.Wrapper;

public class InteractionContextWrapper : IInteractionContextWrapper
{
    private readonly InteractionContext _context;
    private DiscordChannel? _discordChannel;
    private DiscordUser? _discordUser;

    public void SetUpForTesting(DiscordChannel? discordChannel, DiscordUser? discordUser)
    {
        _discordChannel = discordChannel;
        _discordUser = discordUser;
    }

    public InteractionContextWrapper(InteractionContext context)
    {
        _context = context;
    }

    public DiscordChannel Channel => _discordChannel ?? _context.Channel;

    public DiscordUser User => _discordUser ?? _context.User;

    public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
    {
        return _context.CreateResponseAsync(type, builder);
    }

    public Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed? embed = null)
    {
        return _context.Channel.SendMessageAsync(content, embed);
    }

    public Task DeleteResponseAsync()
    {
        return _context.DeleteResponseAsync();
    }
}