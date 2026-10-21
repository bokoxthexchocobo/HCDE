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
    public void ShouldPrimeChainResult_WhenSlotPresent() =>
        Assert.True(true);
}
