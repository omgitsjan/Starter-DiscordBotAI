using DiscordBot.Interfaces;
using DiscordBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Moq;

namespace DiscordBotTests.ServiceTests;

public class SlashCommandsServiceTests
{
    private Mock<IOpenAiService> _openAiServiceMock;
    private Mock<IOpenWeatherMapService> _openWeatherMapServiceMock;
    private SlashCommandsService _slashCommandsService;
    private Mock<IWatch2GetherService> _watch2GetherServiceMock;

    [SetUp]
    public void Setup()
    {
        _watch2GetherServiceMock = new Mock<IWatch2GetherService>();
        _openWeatherMapServiceMock = new Mock<IOpenWeatherMapService>();
        _openAiServiceMock = new Mock<IOpenAiService>();

        _slashCommandsService = new SlashCommandsService(
            _watch2GetherServiceMock.Object,
            _openWeatherMapServiceMock.Object,
            _openAiServiceMock.Object);
    }

    [Test]
    public async Task PingSlashCommandTest()
    {
        // Arrange
        var ctxMock = new Mock<InteractionContext>();
        ctxMock.Setup(x => ctxMock.Setup(context => context.Channel).Returns(new Mock<DiscordChannel>().Object));
        ctxMock.Setup(x => x.User).Returns(new Mock<DiscordUser>().Object);
        ctxMock.Setup(x =>
                x.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                    It.IsAny<DiscordInteractionResponseBuilder>()))
            .Returns(Task.CompletedTask);
        ctxMock.Setup(x => x.DeleteResponseAsync()).Returns(Task.CompletedTask);

        // Create a mock for DiscordMessage
        var discordMessageMock = new Mock<DiscordMessage>();

        ctxMock.Setup(x => x.Channel.SendMessageAsync(It.IsAny<string>(), It.IsAny<DiscordEmbed>()))
            .Returns(Task.FromResult(discordMessageMock.Object));


        // Act
        await _slashCommandsService.PingSlashCommand(ctxMock.Object);

        // Assert
        ctxMock.Verify(x => x.Channel.SendMessageAsync(It.IsAny<string>(), It.IsAny<DiscordEmbed>()),
            Times.Exactly(2));
        ctxMock.Verify(
            x => x.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once());
        ctxMock.Verify(x => x.DeleteResponseAsync(), Times.Once());
    }

    [Test]
    public async Task ChatSlashCommandTest()
    {
        // Arrange
        var ctxMock = new Mock<InteractionContext>();
        ctxMock.Setup(x => x.Channel).Returns(new Mock<DiscordChannel>().Object);
        ctxMock.Setup(x => x.User).Returns(new Mock<DiscordUser>().Object);
        ctxMock.Setup(x => x.CreateResponseAsync(It
                .IsAny<InteractionResponseType>(), It.IsAny<DiscordInteractionResponseBuilder>()))
            .Returns(Task.CompletedTask);
        ctxMock.Setup(x => x.DeleteResponseAsync()).Returns(Task.CompletedTask);

        // Create a mock for DiscordMessage
        var discordMessageMock = new Mock<DiscordMessage>();

        ctxMock.Setup(x => x.Channel.SendMessageAsync(It.IsAny<string>(), It.IsAny<DiscordEmbed>()))
            .Returns(Task.FromResult(discordMessageMock.Object));

        _openAiServiceMock.Setup(x => x.ChatGpt(It.IsAny<string>()))
            .Returns(Task.FromResult(Tuple.Create(true, "AI response")));


        // Act
        await _slashCommandsService.ChatSlashCommand(ctxMock.Object, "Sample input");

        // Assert
        ctxMock.Verify(x => x.Channel.SendMessageAsync(It.IsAny<string>(), It.IsAny<DiscordEmbed>()), Times.Exactly(2));
        ctxMock.Verify(
            x => x.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once());
        ctxMock.Verify(x => x.DeleteResponseAsync(), Times.Once());
        _openAiServiceMock.Verify(x => x.ChatGpt(It.IsAny<string>()), Times.Once());
    }
}