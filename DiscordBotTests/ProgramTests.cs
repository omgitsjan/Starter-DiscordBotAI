using DiscordBot;

namespace DiscordBotTests;

public class ProgramTests
{
    [Test]
    public async Task GetCurrentBitcoinPriceAsync_ReturnsValue()
    {
        // Act
        var result = await Program.GetCurrentBitcoinPriceAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual("Could not fetch current Bitcoin price...", result);
    }

    [Test]
    public async Task GetRandomDeveloperExcuseAsync_ReturnsValue()
    {
        // Act
        var result = await Program.GetRandomDeveloperExcuseAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual("Could not fetch current Developer excuse...", result);
    }
}