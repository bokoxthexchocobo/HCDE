namespace HCDE.Playsim.Tests;

public class VmActionRangeInventoryTests
{
    [Fact]
    public void SquaredRange_Multiplies() => Assert.Equal(9, VmActionRangeInventory.SquaredRange(3));

    [Fact]
    public void ShouldCheckCamera_WhenNonPlayerCamera() =>
        Assert.True(VmActionRangeInventory.ShouldCheckCamera(true, false));

    [Fact]
    public void ShouldDropInventory_WhenFound() =>
        Assert.True(VmActionRangeInventory.ShouldDropInventory(new object(), new object()));

    [Fact]
    public void NormalizeBlendColor_ClearsWhite() =>
        Assert.Equal(0, VmActionRangeInventory.NormalizeBlendColor(VmActionRangeInventory.WhiteArgb));

    [Fact]
    public void CountdownArgInRange_RejectsFive() =>
        Assert.False(VmActionRangeInventory.CountdownArgInRange(5));

    [Fact]
    public void ShouldJumpWhenCountdownZero_AtZero() =>
        Assert.True(VmActionRangeInventory.ShouldJumpWhenCountdownZero(0));

    [Fact]
    public void DecrementCountdownArg_SubtractsOne() =>
        Assert.Equal(2, VmActionRangeInventory.DecrementCountdownArg(3));

    [Fact]
    public void BurstChunkCount_ScalesWithRadius() =>
        Assert.Equal(40, VmActionRangeInventory.BurstChunkCount(20, 64, 0));

    [Fact]
    public void BurstOffsetFromPr_ScalesRadius() =>
        Assert.Equal(0, VmActionRangeInventory.BurstOffsetFromPr(128, 32));

    [Fact]
    public void BurstShardVelocityZ_MatchesNative2105() =>
        Assert.Equal(2, VmActionRangeInventory.BurstShardVelocityZ(16, 0, 32));

    [Fact]
    public void BurstZFromPr_ScalesHeight() =>
        Assert.Equal(64, VmActionRangeInventory.BurstZFromPr(255, 64));

    [Fact]
    public void BurstVelocityFromPr_ScalesRandom2() =>
        Assert.Equal(1.0, VmActionRangeInventory.BurstVelocityFromPr(128).vx);

    [Fact]
    public void BurstChunkNull() =>
        Assert.True(VmActionRangeInventory.BurstChunkNull(null));

    [Fact]
    public void CheckRangeLoop_ReturnsTrueWhenAnyPlayerInRange()
    {
        var hit = VmActionRangeInventory.CheckRangeLoop([(true, true, false, false, false)]);
        Assert.True(hit);
    }
}
