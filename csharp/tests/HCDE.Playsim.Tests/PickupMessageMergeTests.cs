namespace HCDE.Playsim.Tests;

public class PickupMessageMergeTests
{
    [Fact]
    public void ShouldPrint_MergesDuplicateMessagesOnSameTic()
    {
        var lastTic = -1;
        string? last = null;
        Assert.True(PickupMessageMerge.ShouldPrint(true, "Picked up ammo", 5, false, ref lastTic, ref last, out _));
        Assert.False(PickupMessageMerge.ShouldPrint(true, "Picked up ammo", 5, false, ref lastTic, ref last, out _));
    }

    [Fact]
    public void ShouldPrint_StripsDollarPrefixForLookup()
    {
        var lastTic = -1;
        string? last = null;
        Assert.True(PickupMessageMerge.ShouldPrint(true, "$PICKUP", 1, true, ref lastTic, ref last, out var resolved));
        Assert.Equal("PICKUP", resolved);
    }
}
