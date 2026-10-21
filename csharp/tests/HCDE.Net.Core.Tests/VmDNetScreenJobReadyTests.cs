namespace HCDE.Net.Core.Tests;

public class VmDNetScreenJobReadyTests
{
    [Fact]
    public void IsPlayerReadyParticipantNative_RequiresNoBot() =>
        Assert.True(VmDNetScreenJobReady.IsPlayerReadyParticipantNative(cutsceneReadyParticipant: true, hasBot: false));

    [Fact]
    public void ShouldApplyLocalReadiedUpNative_WhenNetgame() =>
        Assert.True(VmDNetScreenJobReady.ShouldApplyLocalReadiedUpNative(
            netgame: true, demoplayback: false, consolePlayer: 0, maxPlayers: 4));

    [Fact]
    public void ShouldWriteReadiedCommandNative_WhenNetgame() =>
        Assert.True(VmDNetScreenJobReady.ShouldWriteReadiedCommandNative(netgame: true, demoplayback: false));
}
