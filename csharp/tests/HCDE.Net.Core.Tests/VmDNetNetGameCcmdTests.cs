namespace HCDE.Net.Core.Tests;

public class VmDNetNetGameCcmdTests
{
    [Fact]
    public void RejectWithoutNetgameNative_WhenOffline() =>
        Assert.True(VmDNetNetGameCcmd.RejectWithoutNetgameNative(false));

    [Fact]
    public void ShouldKickClientNative_WhenOtherPlayerInGame() =>
        Assert.True(VmDNetNetGameCcmd.ShouldKickClientNative(1, 0, inGame: true));

    [Fact]
    public void IsClientMutedNative_DetectsBit() =>
        Assert.True(VmDNetNetGameCcmd.IsClientMutedNative(1ul << 2, 2));

    [Fact]
    public void RejectWithoutAuthorityNative_WhenNotAuthority() =>
        Assert.True(VmDNetNetGameCcmd.RejectWithoutAuthorityNative(false));

    [Fact]
    public void KickArgcValidNative_WhenTwoArgs() =>
        Assert.True(VmDNetNetGameCcmd.KickArgcValidNative(2));
}
