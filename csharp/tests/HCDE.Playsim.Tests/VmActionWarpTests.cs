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
    public void PrimeWarpInventoryReturns_SetsChainWhenSlotPresent()
    {
        var (setChain, retCount) = VmActionWarp.PrimeWarpInventoryReturns(hasChainSlot: true, hasStateSlot: true);
        Assert.True(setChain);
        Assert.Equal(2, retCount);
    }

    [Fact]
    public void UseActorPointerLookup_WhenTidFlagClear() =>
        Assert.True(VmActionWarp.UseActorPointerLookup(0));
}
