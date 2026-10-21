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
    public void PrimeWarpChainResult_WhenSlotPresent() =>
        Assert.True(VmActionWarp.PrimeWarpChainResult(true));
}
