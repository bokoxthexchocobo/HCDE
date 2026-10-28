namespace HCDE.Playsim.Tests;

public class VmActionWarpTests
{
    [Fact]
    public void WarpFailsWithoutReference() =>
        Assert.True(VmActionWarp.WarpFailsWithoutReference(null));

    [Fact]
    public void UseTidLookup_WhenFlagged() =>
        Assert.True(VmActionWarp.UseTidLookup(VmActionWarp.WarpfUsetid));

    [Fact]
    public void PrimeDualReturnCount_WhenMultipleReturns() =>
        Assert.Equal(2, VmActionWarp.PrimeDualReturnCount(3));

    [Fact]
    public void ShouldClearStatePointer_WhenAnyReturn() =>
        Assert.True(VmActionWarp.ShouldClearStatePointer(1));
}
