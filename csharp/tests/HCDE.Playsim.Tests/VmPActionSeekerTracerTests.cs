namespace HCDE.Playsim.Tests;

public class VmPActionSeekerTracerTests
{
    [Fact]
    public void SeekerMissile_StickyPlayerTracerWhenNoLook()
    {
        var target = new object();
        var self = new VmPActionSeekerTracer.SeekerActor
        {
            Target = target,
            TargetIsPlayer = true,
        };
        VmPActionSeekerTracer.SeekerMissile(new(), self, 0, 0, flags: 0, 50, 10);
        Assert.Same(target, self.Tracer);
    }

    [Fact]
    public void SeekerMissile_LookClearsTracerOnMiss()
    {
        var self = new VmPActionSeekerTracer.SeekerActor { Tracer = new() };
        var ctx = new VmPActionSeekerTracer.SeekerContext
        {
            SeekerMissileSteer = (_, _, _, _, _) => false,
        };
        VmPActionSeekerTracer.SeekerMissile(ctx, self, 0, 0, VmPActionSeekerTracer.SmfLook, 50, 10);
        Assert.Null(self.Tracer);
    }

    [Fact]
    public void DoTracer2_SkipsDeadTracer()
    {
        var self = new VmPActionSeekerTracer.TracerActor { TracerHealth = 0, Tracer = new() };
        VmPActionSeekerTracer.DoTracer2(new(), self, 5);
        Assert.Equal(0, self.Yaw);
    }

    [Fact]
    public void DoTracer2_TurnsTowardTracer()
    {
        var self = new VmPActionSeekerTracer.TracerActor
        {
            Tracer = new(),
            TracerHealth = 1,
            Speed = 10,
            CanSeek = true,
            Yaw = 0,
        };
        var ctx = new VmPActionSeekerTracer.TracerContext
        {
            AngleTo = _ => 30,
            DeltaAngle = (_, b) => b - 0,
            VelFromAngle = _ => { },
        };
        VmPActionSeekerTracer.DoTracer2(ctx, self, 5);
        Assert.True(self.Yaw > 0);
    }

    [Fact]
    public void TracerAction_SkipsOffTic()
    {
        var puffs = 0;
        var ctx = new VmPActionSeekerTracer.TracerSmokeContext
        {
            MapTime = 1,
            SpawnPuff = _ => puffs++,
        };
        VmPActionSeekerTracer.TracerAction(ctx, new());
        Assert.Equal(0, puffs);
    }

    [Fact]
    public void CheckIfSeen_ReturnsFalseWhenAnyPlayerSees()
    {
        var ctx = new VmPActionSeekerTracer.SightContext
        {
            PlayerInGame = i => i == 0,
            PlayerMo = _ => new(),
            CheckSight = (_, _) => true,
        };
        Assert.False(VmPActionSeekerTracer.CheckIfSeen(ctx, new()));
    }

    [Fact]
    public void CheckIfSeen_ReturnsTrueWhenUnseen()
    {
        var ctx = new VmPActionSeekerTracer.SightContext { PlayerInGame = _ => false };
        Assert.True(VmPActionSeekerTracer.CheckIfSeen(ctx, new()));
    }
}
