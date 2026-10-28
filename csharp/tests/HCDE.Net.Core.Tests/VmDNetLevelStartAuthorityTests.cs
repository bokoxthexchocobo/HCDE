namespace HCDE.Net.Core.Tests;

public class VmDNetLevelStartAuthorityTests
{
    [Fact]
    public void SetWaitingNative_ResetsLevelStartWhenNetGame()
    {
        Assert.True(VmDNetLevelStartAuthority.SetWaitingNative(
            true, false, true, out var ack, out var latency, out var waiting));
        Assert.True(ack);
        Assert.True(latency);
        Assert.True(waiting);
    }

    [Fact]
    public void SelectNextServiceAuthoritySlotNative_SkipsReserved()
    {
        var slot = VmDNetLevelStartAuthority.SelectNextServiceAuthoritySlotNative(
            new[] { 0, 1, 2 }, 2, c => c == 0, false, 0);
        Assert.Equal(1, slot);
    }

    [Fact]
    public void IsMapLoadedNative_MatchesGameState()
    {
        Assert.True(VmDNetLevelStartAuthority.IsMapLoadedNative(VmDNetLevelStartAuthority.GameStateLevel));
        Assert.False(VmDNetLevelStartAuthority.IsMapLoadedNative(6)); // GS_STARTUP
    }

    [Fact]
    public void LevelStartPlayableMaskNative_ExcludesAuthorityAndLateJoin()
    {
        var mask = VmDNetLevelStartAuthority.LevelStartPlayableMaskNative(
            new[] { 0, 1, 2 },
            c => c == 0,
            c => c == 2);
        Assert.Equal(1ul << 1, mask);
    }

    [Fact]
    public void AuthorityLevelStartStatusAfterReleaseNative_ReturnsReady()
    {
        Assert.Equal(VmDNetLevelStartAuthority.LevelStartReady,
            VmDNetLevelStartAuthority.AuthorityLevelStartStatusAfterReleaseNative());
    }

    [Fact]
    public void PlayableNetworkClientCountNative_ExcludesAuthority()
    {
        var count = VmDNetLevelStartAuthority.PlayableNetworkClientCountNative(
            new[] { 0, 1, 3 }, c => c == 0, _ => false);
        Assert.Equal(2, count);
    }

    [Fact]
    public void HasPlayableNetworkClientsNative_MatchesCount()
    {
        Assert.True(VmDNetLevelStartAuthority.HasPlayableNetworkClientsNative(1));
        Assert.False(VmDNetLevelStartAuthority.HasPlayableNetworkClientsNative(0));
    }
}
