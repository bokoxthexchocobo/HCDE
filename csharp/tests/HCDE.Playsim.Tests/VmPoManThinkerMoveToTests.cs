namespace HCDE.Playsim.Tests;

public class VmPoManThinkerMoveToTests
{
    [Fact]
    public void RotatePolyTick_DestroysWhenDistanceReachesZero()
    {
        var destroyed = false;
        var ctx = new VmPoManThinkerMoveTo.RotatePolyTickContext
        {
            Dist = 5,
            Speed = 5,
            RotatePolyobj = _ => true,
            Destroy = () => destroyed = true,
        };
        VmPoManThinkerMoveTo.RotatePolyTick(ctx);
        Assert.True(destroyed);
    }

    [Fact]
    public void MovePolyTick_ClampsSpeedOnFinalStep()
    {
        var ctx = new VmPoManThinkerMoveTo.MovePolyTickContext
        {
            Dist = 12,
            Speed = 10,
            AngleDegrees = 0,
            SpeedVx = 10,
            MovePolyobj = (_, _) => true,
        };
        VmPoManThinkerMoveTo.MovePolyTick(ctx);
        Assert.Equal(2, ctx.Speed);
    }

    [Fact]
    public void EvMovePolyTo_StartsOnMirrorChain()
    {
        var started = new List<int>();
        var ctx = new VmPoManThinkerMoveTo.EvMovePolyToContext
        {
            ResolveMirror = id => id switch
            {
                1 => new VmPoManThinkerMoveTo.MovePolyToPolyState { PolyId = 1, StartX = 0, StartY = 0, MirrorPolyId = 0 },
                _ => null,
            },
            SpawnThinker = (m, _, _, _, _, _, _) => started.Add(m.PolyId),
        };
        Assert.True(VmPoManThinkerMoveTo.EvMovePolyTo(ctx, 1, 10, 0, 4));
        Assert.Equal(new[] { 1 }, started);
    }
}
