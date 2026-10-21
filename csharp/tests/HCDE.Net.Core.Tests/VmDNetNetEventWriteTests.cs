namespace HCDE.Net.Core.Tests;

public class VmDNetNetEventWriteTests
{
    [Fact]
    public void RejectAdoptRoomWhenAuthorityNative_Blocks() =>
        Assert.True(VmDNetNetEventWrite.RejectAdoptRoomWhenAuthorityNative(true));

    [Fact]
    public void GrowBufferLengthNative_RoundsUp() =>
        Assert.Equal(256, VmDNetNetEventWrite.GrowBufferLengthNative(200, 0));

    [Fact]
    public void SelectWeaponLevelGateNative_WhenLive() =>
        Assert.True(VmDNetNetEventWrite.SelectWeaponLevelGateNative(
            gameState: 1, levelState: 1, paused: false, playerState: 0, liveState: 0));
}
