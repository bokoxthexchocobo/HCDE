namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandSummonTests
{
    [Fact]
    public void ShouldSpawnPlayerMissileNative_WhenMissileFlag() =>
        Assert.True(VmDNetDoCommandSummon.ShouldSpawnPlayerMissileNative(mobjFlags: 1, mfMissile: 1));

    [Fact]
    public void ShouldApplySummonFriendFlagsNative_ForFriendOpcode() =>
        Assert.True(VmDNetDoCommandSummon.ShouldApplySummonFriendFlagsNative(
            cmd: 10, demSummonFriend: 10, demSummonFriend2: 12, demSummonMbf: 14));

    [Fact]
    public void ShouldRunNamedScriptNative_OnLevel() =>
        Assert.True(VmDNetDoCommandSummon.ShouldRunNamedScriptNative(gameState: 2, levelState: 2));

    [Fact]
    public void ShouldOfferAutosaveNative_WhenEligible() =>
        Assert.True(VmDNetDoCommandSummon.ShouldOfferAutosaveNative(
            netgame: false, demoplayback: false, disableAutosave: 0, autosaveCount: 1,
            playerState: 0, liveState: 0, deathmatch: false));
}
