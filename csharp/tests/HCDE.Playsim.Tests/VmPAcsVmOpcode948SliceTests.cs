using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode948SliceTests
{
    [Fact]
    public void ResolvePlayMovieStackValue_ForcesNegativeOne() =>
        Assert.Equal(-1, VmPAcsVmOpcode948Slice.ResolvePlayMovieStackValue(2));

    [Fact]
    public void SetActorPositionInterpolate_NonZeroIsTrue() =>
        Assert.True(VmPAcsVmOpcode948Slice.SetActorPositionInterpolate(1));

    [Fact]
    public void ResolveActorZWithBob_IncludesBobOffset() =>
        Assert.Equal(VmPAcsVmOpcode948Slice.DoubleToAcs(3.5), VmPAcsVmOpcode948Slice.ResolveActorZWithBob(new object(), 3.0, 0.5));

    [Fact]
    public void SelectActorAxisAcs_UsesYForGetActorY() =>
        Assert.Equal(VmPAcsVmOpcode948Slice.DoubleToAcs(2.0), VmPAcsVmOpcode948Slice.SelectActorAxisAcs((int)AcsPcode.GetActorY, 1.0, 2.0));

    [Fact]
    public void SectorHeightUsesPointInSector_WhenTagZero() =>
        Assert.True(VmPAcsVmOpcode948Slice.SectorHeightUsesPointInSector(0));

    [Fact]
    public void ResolveSectorLightLevel_ReturnsNegativeOneWhenMissing() =>
        Assert.Equal(-1, VmPAcsVmOpcode948Slice.ResolveSectorLightLevel(-1, 128));

    [Fact]
    public void IsCeilingPlaneWatcher_ForSetCeilingTrigger() =>
        Assert.True(VmPAcsVmOpcode948Slice.IsCeilingPlaneWatcher((int)AcsPcode.SetCeilingTrigger));

    [Fact]
    public void TranslationTableSlot_IsZeroBased() =>
        Assert.Equal(4, VmPAcsVmOpcode948Slice.TranslationTableSlot(5));

    [Fact]
    public void ApplyPlaneWatcherStackPop_DropsEightSlots()
    {
        var sp = 10;
        VmPAcsVmOpcode948Slice.ApplyPlaneWatcherStackPop(ref sp);
        Assert.Equal(2, sp);
    }
}
