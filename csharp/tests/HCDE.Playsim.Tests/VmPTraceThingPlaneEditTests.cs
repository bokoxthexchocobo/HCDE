namespace HCDE.Playsim.Tests;

public class VmPTraceThingPlaneEditTests
{
    [Fact]
    public void ThingCheck_HitsActorWhenNo3DFloors()
    {
        var s = new VmPTraceThingPlaneEdit.ThingCheckState
        {
            HitZ = 32,
            ThingZ = 0,
            ThingTop = 64,
            FfFloorCount = 0,
            ResultActor = new object(),
        };
        Assert.False(VmPTraceThingPlaneEdit.ThingCheck(s));
        Assert.Equal(VmPTraceThingPlaneEdit.TraceHitActor, s.ResultHitType);
    }

    [Fact]
    public void ThingCheck_3DFloorCeilingBlocksBeforeActor()
    {
        var s = new VmPTraceThingPlaneEdit.ThingCheckState
        {
            HitZ = 100,
            ThingTop = 64,
            FfFloorCount = 1,
            FfCeilingZ = 90,
            PortalBlocksCeiling = true,
            CeilingTexture = 7,
            HasTraceCallback = true,
            TraceCallback = () => VmPTraceThingPlaneEdit.TraceStop,
        };
        Assert.False(VmPTraceThingPlaneEdit.ThingCheck(s));
        Assert.Equal(VmPTraceThingPlaneEdit.TraceHitCeiling, s.ResultHitType);
    }

    [Fact]
    public void CheckPlane_RequiresEnterDistWindow()
    {
        var s = new VmPTraceThingPlaneEdit.CheckPlaneState
        {
            Den = 1,
            Num = -5,
            EnterDist = 1,
            MaxDist = 10,
            VecX = 1,
        };
        Assert.True(VmPTraceThingPlaneEdit.CheckPlane(s));
        Assert.Equal(5, s.ResultDistance);
    }

    [Fact]
    public void EditTraceResult_NoSkyClearsSkyFlatHit()
    {
        var s = new VmPTraceThingPlaneEdit.EditTraceResultState
        {
            Flags = VmPTraceThingPlaneEdit.TraceNoSky,
            HitType = VmPTraceThingPlaneEdit.TraceHitFloor,
            HitTexture = 1,
            SkyFlat = 1,
        };
        Assert.False(VmPTraceThingPlaneEdit.EditTraceResult(s));
        Assert.Equal(VmPTraceThingPlaneEdit.TraceHitNone, s.HitType);
    }

    [Fact]
    public void EditTraceResult_HitSkyMarksHasHitSky()
    {
        var s = new VmPTraceThingPlaneEdit.EditTraceResultState
        {
            Flags = VmPTraceThingPlaneEdit.TraceHitSky,
            HitType = VmPTraceThingPlaneEdit.TraceHitCeiling,
            HitTexture = 2,
            SkyFlat = 2,
        };
        Assert.True(VmPTraceThingPlaneEdit.EditTraceResult(s));
        Assert.Equal(VmPTraceThingPlaneEdit.TraceHasHitSky, s.HitType);
    }
}
