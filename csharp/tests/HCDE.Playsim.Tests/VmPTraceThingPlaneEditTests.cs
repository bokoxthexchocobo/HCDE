namespace HCDE.Playsim.Tests;

public class VmPTraceThingPlaneEditTests
{
    [Fact]
    public void EditTraceResult_NoSkyClearsSkyFlatHit()
    {
        Assert.False(VmPTraceThingPlaneEdit.EditTraceResult(2, 2, 1, 1, false));
        Assert.True(VmPTraceThingPlaneEdit.EditTraceResult(0, 2, 1, 1, false));
    }

    [Fact]
    public void ThingCheckPassesTopEntry_WhenBelowTopOrMovingDown()
    {
        Assert.True(VmPTraceThingPlaneEdit.ThingCheckPassesTopEntry(10, 20, -1));
        Assert.False(VmPTraceThingPlaneEdit.ThingCheckPassesTopEntry(25, 20, 1));
    }

    [Fact]
    public void CheckPlaneHits_WhenRayIntersectsPlaneSegment()
    {
        Assert.True(VmPTraceThingPlaneEdit.CheckPlaneHits(2, 4, 10, 1));
        Assert.False(VmPTraceThingPlaneEdit.CheckPlaneHits(0, 4, 10, 1));
    }
}
