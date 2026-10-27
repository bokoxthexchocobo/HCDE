namespace HCDE.Playsim.Tests;

public class VmPLnspecLineSpecialDispatch935SliceTests
{
    [Fact]
    public void ContinuedDispatchEntryCount_Is150() =>
        Assert.Equal(150, VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchEntryCount());

    [Fact]
    public void ContinuedDispatchName_MapsTeleportOpcode() =>
        Assert.Equal("LS_Teleport", VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchName(70));

    [Fact]
    public void ContinuedDispatchIsAcs_RecognizesExecute() =>
        Assert.True(VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchIsAcs(80));

    [Fact]
    public void ContinuedDispatchIsNop_ScrollTextureSlot() =>
        Assert.True(VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchIsNop(100));

    [Fact]
    public void ContinuedDispatchName_TableSlotForSetPlayerProperty() =>
        Assert.Equal("LS_SetPlayerProperty", VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchName(191));

    [Fact]
    public void ContinuedDispatchName_EndsAtPerpetualRaiseLip() =>
        Assert.Equal(
            "LS_Plat_PerpetualRaiseLip",
            VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchName(207));

    [Fact]
    public void ContinuedDispatchIsLight_IncludesStrobe() =>
        Assert.True(VmPLnspecLineSpecialDispatch935Slice.ContinuedDispatchIsLight(114));
}
