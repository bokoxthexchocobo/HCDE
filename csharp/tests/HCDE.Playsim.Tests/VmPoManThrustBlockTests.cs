namespace HCDE.Playsim.Tests;

public class VmPoManThrustBlockTests
{
    [Fact]
    public void ThrustMobj_AddsVelocityFromSideNormal()
    {
        var ctx = new VmPoManThrustBlock.ThrustMobjContext
        {
            Shootable = true,
            SideV1X = 0,
            SideV1Y = 0,
            SideV2X = 1,
            SideV2Y = 0,
            PolySpeed = 8,
        };
        VmPoManThrustBlock.ThrustMobj(ctx);
        Assert.True(Math.Abs(ctx.VelY) > 0);
    }

    [Fact]
    public void ThrustMobj_SkipsNonShootableNonPlayer()
    {
        var ctx = new VmPoManThrustBlock.ThrustMobjContext { Shootable = false, IsPlayer = false, VelX = 5 };
        VmPoManThrustBlock.ThrustMobj(ctx);
        Assert.Equal(5, ctx.VelX);
    }

    [Fact]
    public void CheckMobjBlocking_ThrustsSolidActorInRange()
    {
        var thrusts = new List<int>();
        var ctx = new VmPoManThrustBlock.CheckMobjBlockingContext
        {
            BlockWidth = 4,
            BlockHeight = 4,
            BoxLeft = 0,
            BoxRight = 0,
            BoxTop = 0,
            BoxBottom = 0,
            MobjsInBlock = (_, _) => new[]
            {
                new VmPoManThrustBlock.BlockingMobj(true, false, 1, 1, 16, 0, 64, 0, 7),
            },
            InRange = (_, _, _, _, _) => true,
            BoxOnLineSide = _ => -1,
            Thrust = m => thrusts.Add(m.Id),
        };
        Assert.True(VmPoManThrustBlock.CheckMobjBlocking(ctx));
        Assert.Equal(new[] { 7 }, thrusts);
    }

    [Fact]
    public void CheckMobjBlocking_SkipsDuplicateMobj()
    {
        var thrusts = 0;
        var mobj = new VmPoManThrustBlock.BlockingMobj(true, false, 1, 1, 16, 0, 64, 0, 1);
        var ctx = new VmPoManThrustBlock.CheckMobjBlockingContext
        {
            BlockWidth = 2,
            BlockHeight = 2,
            BoxLeft = 0,
            BoxRight = 1,
            BoxTop = 0,
            BoxBottom = 0,
            MobjsInBlock = (_, _) => new[] { mobj, mobj },
            InRange = (_, _, _, _, _) => true,
            BoxOnLineSide = _ => -1,
            Thrust = _ => thrusts++,
        };
        VmPoManThrustBlock.CheckMobjBlocking(ctx);
        Assert.Equal(1, thrusts);
    }
}
