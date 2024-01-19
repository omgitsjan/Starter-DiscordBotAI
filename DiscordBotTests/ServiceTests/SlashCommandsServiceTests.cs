using DiscordBot.Interfaces;
using DiscordBot.Services;
using DiscordBot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Moq;

namespace DiscordBotTests.ServiceTests;

[Ignore("This test is not yet implemented. Currently, I don't know exactly how to properly mock the DiscordUser & DiscordChannel.")]
public class SlashCommandsServiceTests
{
    private Mock<IInteractionContextWrapper> _ctxMock = null!;
    private Mock<IOpenAiService> _openAiServiceMock = null!;
    private Mock<IOpenWeatherMapService> _openWeatherMapServiceMock = null!;
    private SlashCommandsService _slashCommandsService = null!;
    private Mock<IWatch2GetherService> _watch2GetherServiceMock = null!;
    private Mock<ICryptoService> _cryptoServiceMock = null!;
        
    [SetUp]
    public void Setup()
    {
        _ctxMock = new Mock<IInteractionContextWrapper>();
        _watch2GetherServiceMock = new Mock<IWatch2GetherService>();
        _openWeatherMapServiceMock = new Mock<IOpenWeatherMapService>();
        _openAiServiceMock = new Mock<IOpenAiService>();
        _cryptoServiceMock = new Mock<ICryptoService>();

        _slashCommandsService = new SlashCommandsService(_watch2GetherServiceMock.Object,
            _openWeatherMapServiceMock.Object, _openAiServiceMock.Object, _cryptoServiceMock.Object);
    }

    [Test]
    public async Task TestPingSlashCommandAsync()
    {
        // Arrange
        var channelMock = new Mock<DiscordChannel>(1234567890, null) { CallBase = true };

        var userMock = new Mock<DiscordUser>(1234567890, null, null, null, null) { CallBase = true };

        channelMock.SetupGet(c => c.Name).Returns("test-channel");
        userMock.SetupGet(u => u.Username).Returns("test-user");


        _ctxMock.Object.SetUpForTesting(channelMock.Object, userMock.Object);

        // Act
        await _slashCommandsService.PingSlashCommandAsync(_ctxMock.Object);

        // Assert
        _ctxMock.Verify(
            ctx => ctx.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once);
        _ctxMock.Verify(ctx => ctx.DeleteResponseAsync(), Times.Once);
        _ctxMock.Verify(ctx => ctx.Channel.SendMessageAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public async Task TestChatSlashCommandAsync_Success()
    {
        // Arrange
        _openAiServiceMock.Setup(service => service.ChatGptAsync(It.IsAny<string>()))
            .ReturnsAsync(Tuple.Create(true, "Sample ChatGPT Response"));

        // Act
        await _slashCommandsService.ChatSlashCommandAsync(_ctxMock.Object, "Sample Text");

        // Assert
        _ctxMock.Verify(
            ctx => ctx.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once);
        _ctxMock.Verify(ctx => ctx.DeleteResponseAsync(), Times.Once);
        _ctxMock.Verify(ctx => ctx.Channel.SendMessageAsync(It.IsAny<string>()), Times.Exactly(2));
        _openAiServiceMock.Verify(service => service.ChatGptAsync("Sample Text"), Times.Once);
    }

    [Test]
    public async Task TestChatSlashCommandAsync_Failure()
    {
        // Arrange
        _openAiServiceMock.Setup(service => service.ChatGptAsync(It.IsAny<string>()))
            .ReturnsAsync(Tuple.Create(false, "Error"));

        // Act
        await _slashCommandsService.ChatSlashCommandAsync(_ctxMock.Object, "Sample Text");

        // Assert
        _ctxMock.Verify(
            ctx => ctx.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once);
        _ctxMock.Verify(ctx => ctx.DeleteResponseAsync(), Times.Once);
        _ctxMock.Verify(ctx => ctx.Channel.SendMessageAsync(It.IsAny<string>()), Times.Exactly(2));
        _openAiServiceMock.Verify(service => service.ChatGptAsync("Sample Text"), Times.Once);
    }

    [Test]
    public async Task TestImageSlashCommandAsync_Success()
    {
        // Arrange
        _openAiServiceMock.Setup(service => service.DallEAsync(It.IsAny<string>()))
            .ReturnsAsync(Tuple.Create(true, "https://sample-image-url.com/sample-image.jpg"));

        // Act
        await _slashCommandsService.ImageSlashCommandAsync(_ctxMock.Object, "Sample Text");

        // Assert
        _ctxMock.Verify(
            ctx => ctx.CreateResponseAsync(It.IsAny<InteractionResponseType>(),
                It.IsAny<DiscordInteractionResponseBuilder>()), Times.Once);
        _ctxMock.Verify(ctx => ctx.DeleteResponseAsync(), Times.Once);
        _ctxMock.Verify(ctx => ctx.Channel.SendMessageAsync(It.IsAny<string>()), Times.Exactly(2));
        _openAiServiceMock.Verify(service => service.DallEAsync("Sample Text"), Times.Once);
    }
}