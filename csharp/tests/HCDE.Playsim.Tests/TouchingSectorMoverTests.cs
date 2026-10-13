namespace HCDE.Playsim.Tests;

public class TouchingSectorMoverTests
{
    [Theory]
    [InlineData(-8, 32)]
    [InlineData(-40, 40)]
    public void ActorLandingSnapsBeforeTouchingCeilingFit(int x, int expectedZ)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 64); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(x); actor.Height = Fixed.FromInt(32);
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        var support = sim.AddBot(x, 0); support.Brain = null; support.Height = Fixed.FromInt(40);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expectedZ, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.OnMobj);
    }

    [Fact]
    public void TouchingCeilingLandingSaveContinuation()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 64); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(-8); actor.Height = Fixed.FromInt(32);
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        var support = sim.AddBot(-8, 0); support.Brain = null; support.Height = Fixed.FromInt(40);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(32, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(-8, true)]
    [InlineData(-40, false)]
    public void OnMobjProbeUsesTouchingCeiling(int x, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 64); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(x); actor.Z = Fixed.FromInt(16); actor.VelocityZ = Fixed.FromInt(-1);
        var support = sim.AddBot(x, 0); support.Brain = null; support.Height = Fixed.FromInt(12);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-8, false)]
    [InlineData(-40, true)]
    public void OnMobjProbeUsesTouchingFloor(int x, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(16, 128); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(x); actor.Z = Fixed.FromInt(8); actor.VelocityZ = default;
        var support = sim.AddBot(x, 0); support.Brain = null; support.Z = default; support.Height = Fixed.FromInt(12);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(MotionKind.Ceiling, -8, true)]
    [InlineData(MotionKind.Door, -8, true)]
    [InlineData(MotionKind.Floor, -8, true)]
    [InlineData(MotionKind.Ceiling, -40, false)]
    [InlineData(MotionKind.Floor, -40, false)]
    public void AdjacentMoverDetectsActorCrossingBoundary(MotionKind kind, int x, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 64); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(x); Assert.Equal(0, actor.SectorIndex);
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = kind,
            CeilingDirection = -1, TargetCeiling = 0, ClosedCeiling = 0,
            Closing = kind == MotionKind.Door, FloorDirection = 1, TargetFloor = 32, Speed = 16, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(kind == MotionKind.Floor && !blocked ? 16 : 0, sim.FloorOf(1));
        Assert.Equal(kind != MotionKind.Floor && !blocked ? 48 : 64, sim.CeilingOf(1));
    }

    [Fact]
    public void AdjacentCrusherDamagesTouchingActorOnce()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 64); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(-8);
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Ceiling,
            CeilingDirection = -1, TargetCeiling = 0, Speed = 16, CrushDamage = 10 });
        LineSpecials.TickMotions(sim); Assert.Equal(90, actor.Health); Assert.Equal(48, sim.CeilingOf(1));
    }

    [Fact]
    public void AdjacentFloorDropCarriesOnlyWhenSupportChanges()
    {
        var sim = GameplayFoundationTests.TwoRooms(16, 128); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(-8); actor.Z = Fixed.FromInt(16);
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Floor,
            FloorDirection = -1, TargetFloor = -16, Speed = 8 });
        LineSpecials.TickMotions(sim); Assert.Equal(8, actor.Z.ToDouble());
        LineSpecials.TickMotions(sim); Assert.Equal(0, actor.Z.ToDouble());
        LineSpecials.TickMotions(sim); Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void AdjacentRelativeRaiseUsesGeometricSupportDelta()
    {
        var sim = GameplayFoundationTests.TwoRooms(-16, 256); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(-8); actor.Z = Fixed.FromInt(32); actor.NoGravity = actor.RelativeToFloor = true;
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Floor,
            FloorDirection = 1, TargetFloor = 32, Speed = 16 });
        LineSpecials.TickMotions(sim); Assert.Equal(32, actor.Z.ToDouble());
        LineSpecials.TickMotions(sim); Assert.Equal(48, actor.Z.ToDouble());
    }
}
