using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode947SliceTests
{
    [Fact]
    public void CheckInventoryDirectPcAfterRead_AdvancesOneWord() =>
        Assert.Equal(11, VmPAcsVmOpcode947Slice.CheckInventoryDirectPcAfterRead(10));

    [Fact]
    public void UseActorInventoryUsesWorld_WhenTidZero() =>
        Assert.True(VmPAcsVmOpcode947Slice.UseActorInventoryUsesWorld(0));

    [Fact]
    public void AccumulateUseInventoryResult_SumsUses() =>
        Assert.Equal(3, VmPAcsVmOpcode947Slice.AccumulateUseInventoryResult(1, 2));

    [Fact]
    public void ResolveAmmoCapacity_UsesDefaultWhenNotOwned() =>
        Assert.Equal(50, VmPAcsVmOpcode947Slice.ResolveAmmoCapacity(true, true, false, 200, 50));

    [Fact]
    public void ResolveSigilPieces_ReturnsZeroWithoutSigil() =>
        Assert.Equal(0, VmPAcsVmOpcode947Slice.ResolveSigilPieces(new object(), null, 3));

    [Fact]
    public void TypeParentIsAmmo_MatchesParentId() =>
        Assert.True(VmPAcsVmOpcode947Slice.TypeParentIsAmmo(4, 4));

    [Fact]
    public void InventoryAmountAfterGrant_ClearsPrevious() =>
        Assert.Equal(0, VmPAcsVmOpcode947Slice.InventoryAmountAfterGrant(6));

    [Fact]
    public void ApplySetMusicStackPop_DropsThreeSlots()
    {
        var sp = 6;
        VmPAcsVmOpcode947Slice.ApplySetMusicStackPop(ref sp);
        Assert.Equal(3, sp);
    }

    [Fact]
    public void ShouldCancelFader_WhenActivatorNull() =>
        Assert.True(VmPAcsVmOpcode947Slice.ShouldCancelFader(true, false));

    [Fact]
    public void ResolvePlayMovieStackValue_ForcesNegativeOne() =>
        Assert.Equal(-1, VmPAcsVmOpcode947Slice.ResolvePlayMovieStackValue(4));

    [Fact]
    public void SetsMusic_IncludesLocalDirectOpcode() =>
        Assert.True(VmPAcsVmOpcode947Slice.SetsMusic((int)AcsPcode.LocalSetMusicDirect));

    [Fact]
    public void ApplyFadeRangeStackPop_DropsNineSlots()
    {
        var sp = 12;
        VmPAcsVmOpcode947Slice.ApplyFadeRangeStackPop(ref sp);
        Assert.Equal(3, sp);
    }
}
