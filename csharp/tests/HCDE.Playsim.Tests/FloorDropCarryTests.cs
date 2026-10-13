using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorDropCarryTests
{
    [Theory]
    [InlineData(8, false, false, true)]
    [InlineData(9, false, false, false)]
    [InlineData(16, false, false, false)]
    [InlineData(16, true, false, true)]
    [InlineData(16, false, true, true)]
    public void CarryUsesNativeDropBoundaryAndFlags(double speed, bool moveWithSector, bool noGravity, bool carried)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.MoveWithSector = moveWithSector; actor.NoGravity = noGravity;
        Drop(sim, speed); sim.Tick();
        Assert.Equal(64 - speed, sim.FloorOf(0));
        Assert.Equal(carried ? 64 - speed : 64, actor.Z.ToDouble());
        Assert.Equal(carried, actor.OnGround);
    }

    [Fact]
    public void FinalShortDropUsesActualDistanceRatherThanConfiguredSpeed()
    {
        var sim = Room(); Drop(sim, 16, target: 60); sim.Tick();
        Assert.Equal(60, sim.FloorOf(0)); Assert.Equal(60, sim.Players.Single().Z.ToDouble());
        Assert.Empty(sim.Motions);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void LinkEligibilityControlsCarry(bool noBlockmap, bool moveWithSector, bool carried)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, blockmap: noBlockmap ? 1 : 0);
        actor.MoveWithSector = moveWithSector;
        Drop(sim, 16); LineSpecials.TickMotions(sim);
        Assert.Equal(carried ? 48 : 64, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnlinkedActorsAreNotCarried(bool moveWithSector)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.NoSector = true; actor.MoveWithSector = moveWithSector;
        Drop(sim, 8); sim.Tick();
        Assert.Equal(64, actor.Z.ToDouble()); Assert.False(actor.OnGround);
    }

    [Fact]
    public void BridgeIsNotPulledDownByFloor()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.ActsLikeBridge = true;
        Drop(sim, 8); sim.Tick();
        Assert.Equal(64, actor.Z.ToDouble()); Assert.False(actor.OnGround);
    }

    [Theory]
    [InlineData(false, true, 48)]
    [InlineData(true, true, 72)]
    [InlineData(false, false, 72)]
    public void AirborneCarryRequiresGravityAndMoveWithSector(bool noGravity, bool moveWithSector, double expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(72); actor.OnGround = false;
        actor.NoGravity = noGravity; actor.MoveWithSector = moveWithSector;
        Drop(sim, 16); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Fact]
    public void MovingVerticallyIsNotSnappedByDrop()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.MoveWithSector = true; actor.VelocityZ = Fixed.FromInt(1);
        Drop(sim, 8); LineSpecials.TickMotions(sim);
        Assert.Equal(64, actor.Z.ToDouble()); Assert.Equal(1, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void DroppedFloorSaveRestoresFallingActorWithoutSnapping()
    {
        var sim = Room(); Drop(sim, 16); sim.Tick();
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick();
        Assert.Equal(next, SimSavegame.Write(sim)); Assert.False(sim.Players.Single().OnGround);
    }

    private static void Drop(AuthoritySimulation sim, double speed, double target = 0) =>
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor,
            TargetFloor = target, FloorDirection = -1, Speed = speed });

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { FloorHeight = 64, CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
