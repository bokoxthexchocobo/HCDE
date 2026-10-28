namespace HCDE.Playsim.Tests;

public class VmPLnspecLineSpecialDispatch936SliceTests
{
    [Fact]
    public void TailDispatchEntryCount_Is77() =>
        Assert.Equal(77, VmPLnspecLineSpecialDispatch936Slice.TailDispatchEntryCount());

    [Fact]
    public void TailDispatchName_StartsAtTranslucentLine() =>
        Assert.Equal("LS_TranslucentLine", VmPLnspecLineSpecialDispatch936Slice.TailDispatchName(208));

    [Fact]
    public void TailDispatchName_ChangeCameraIsTableSlotOnly() =>
        Assert.Equal("LS_ChangeCamera", VmPLnspecLineSpecialDispatch936Slice.TailDispatchName(237));

    [Fact]
    public void TailDispatchName_EndsAtGenericCrusherDist() =>
        Assert.Equal("LS_Generic_CrusherDist", VmPLnspecLineSpecialDispatch936Slice.TailDispatchName(284));

    [Fact]
    public void ScanMaxSpecialNumber_FindsLargest() =>
        Assert.Equal(284, VmPLnspecLineSpecialDispatch936Slice.ScanMaxSpecialNumber([12, 284, 90]));

    [Fact]
    public void MaxLineSpecialFromRegistrySize_MatchesNative() =>
        Assert.Equal(284, VmPLnspecLineSpecialDispatch936Slice.MaxLineSpecialFromRegistrySize(285));

    [Fact]
    public void CanAssignRegistrySlot_RejectsOutOfRange() =>
        Assert.False(VmPLnspecLineSpecialDispatch936Slice.CanAssignRegistrySlot(300, 285));
}
