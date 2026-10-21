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
}
