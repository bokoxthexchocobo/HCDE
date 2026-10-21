namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandChatTests
{
    [Fact]
    public void ShouldDropSayNative_WhenChatDisabled() =>
        Assert.True(VmDNetDoCommandChat.ShouldDropSayNative(VmDNetDoCommandChat.ChatDisabled, mutedMask: 0, player: 0));

    [Fact]
    public void FormatTauntSoundNative_UsesVariant() =>
        Assert.Equal("*taunt-wave", VmDNetDoCommandChat.FormatTauntSoundNative("wave"));

    [Fact]
    public void ShouldExecuteGiveCheatNative_AllowsWhenNotRejected() =>
        Assert.True(VmDNetDoCommandChat.ShouldExecuteGiveCheatNative(rejectPredator: false));

    [Fact]
    public void ShouldExecuteWarpCheatNative_AllowsWhenNotRejected() =>
        Assert.True(VmDNetDoCommandChat.ShouldExecuteWarpCheatNative(rejectPredator: false));

    [Fact]
    public void ShouldExecuteGenericCheatNative_AllowsWhenNotRejected() =>
        Assert.True(VmDNetDoCommandChat.ShouldExecuteGenericCheatNative(rejectPredator: false));

    [Fact]
    public void ApplyChangemapCheatFlagNative_SetsBit() =>
        Assert.Equal(5, VmDNetDoCommandChat.ApplyChangemapCheatFlagNative(1, 4));

    [Fact]
    public void ShouldSetCenteringOnCenterviewCommandNative_WhenCenterview() =>
        Assert.True(VmDNetDoCommandChat.ShouldSetCenteringOnCenterviewCommandNative(cmd: 42, centerviewCmd: 42));

    [Fact]
    public void ShouldRunSuicideCommandNative_WhenAlive() =>
        Assert.True(VmDNetDoCommandChat.ShouldRunSuicideCommandNative(
            gameState: 2, levelState: 2, paused: false, playerState: 0, deadState: 4));

}
