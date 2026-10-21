namespace HCDE.Net.Core.Tests;

public class VmDNetCcmdConsoleTests
{
    [Fact]
    public void RejectWithoutNetgameNative_WhenOffline() =>
        Assert.True(VmDNetCcmdConsole.RejectWithoutNetgameNative(false));

    [Fact]
    public void InvasionSpotSpawnReadyPrintNative_WhenReady() =>
        Assert.Equal(1, VmDNetCcmdConsole.InvasionSpotSpawnReadyPrintNative(gametic: 10, nextSpawnGameTic: 5));

    [Fact]
    public void ShouldEnqueueKickTargetNative_WhenValid() =>
        Assert.True(VmDNetCcmdConsole.ShouldEnqueueKickTargetNative(
            target: 1, consolePlayer: 0, existingListSize: 0, foundIndex: 0));

    [Fact]
    public void ApplyMuteClientMaskNative_SetsBit() =>
        Assert.Equal(1ul << 2, VmDNetCcmdConsole.ApplyMuteClientMaskNative(0, 2));
}
