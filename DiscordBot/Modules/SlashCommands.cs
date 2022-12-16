using Discord.Interactions;
using DiscordBot.Services;

namespace DiscordBot.Service;

// interation modules must be public and inherit from an IInterationModuleBase
public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private CommandHandler _handler;

    // constructor injection is also a valid way to access the dependecies
    public SlashCommands(CommandHandler handler)
    {
        _handler = handler;
    }

    // dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
    public InteractionService Commands { get; set; }

    // our first /command!
    [SlashCommand("8ball", "find your answer!")]
    public async Task EightBall(string question)
    {
        // create a list of possible replies
        var replies = new List<string>
        {
            // add our possible replies
            "maybe",
            "hazzzzy...."
        };

        // get the answer
        var answer = replies[new Random().Next(replies.Count - 1)];

        // reply with the answer
        await RespondAsync($"You asked: [**{question}**], and your answer is: [**{answer}**]");
    }
}