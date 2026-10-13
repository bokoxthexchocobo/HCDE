using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RelativeFloorRaiseTests
{
    [Theory]
    [InlineData(true, true, false, 0, 96)]
    [InlineData(true, true, true, 0, 96)]
    [InlineData(true, true, false, 2, 96)]
    [InlineData(true, false, false, 0, 80)]
    [InlineData(false, true, false, 0, 80)]
    public void AirborneRelativeActorFollowsRisingFloor(bool noGravity, bool relative, bool noLiftDrop,
        int velocity, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(80); actor.NoGravity = noGravity; actor.RelativeToFloor = relative;
        actor.NoLiftDrop = noLiftDrop; actor.VelocityZ = Fixed.FromInt(velocity);
        Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(expected, actor.Z.ToDouble());
        Assert.Equal(velocity, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(true, false, false, 80)]
    [InlineData(false, true, false, 80)]
    [InlineData(false, true, true, 96)]
    public void RelativeRaiseHonorsSectorMoverEligibility(bool noSector, bool noBlockmap, bool moveWithSector, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(80); actor.NoGravity = actor.RelativeToFloor = true;
        actor.NoSector = noSector; actor.NoBlockmap = noBlockmap; actor.MoveWithSector = moveWithSector;
        Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false, 16, 80)]
    [InlineData(true, 0, 80)]
    public void ObstructionUsesPredictedRelativeHeight(bool relative, int expectedFloor, int expectedZ)
    {
        var sim = Room(ceiling: 144); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(80); actor.NoGravity = true; actor.RelativeToFloor = relative;
        Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(expectedFloor, sim.FloorOf(0)); Assert.Equal(expectedZ, actor.Z.ToDouble());
        Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(false, 16, 96)]
    [InlineData(true, 0, 80)]
    public void RelativeCrushPreservesMoverStopPolicy(bool stop, int expectedFloor, int expectedZ)
    {
        var sim = Room(ceiling: 144); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(80); actor.NoGravity = actor.RelativeToFloor = true;
        Raise(sim, crush: 10, stop: stop); LineSpecials.TickMotions(sim);
        Assert.Equal(expectedFloor, sim.FloorOf(0)); Assert.Equal(expectedZ, actor.Z.ToDouble());
        Assert.Equal(90, actor.Health);
    }

    [Fact]
    public void FloorIntersectionTakesPrecedenceOverRelativeShift()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(8); actor.NoGravity = actor.RelativeToFloor = true;
        Raise(sim); sim.Tick(); Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnGround);
    }

    [Fact]
    public void SaveRestoreContinuesRelativeRiseExactly()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(80); actor.OnGround = false; actor.NoGravity = actor.RelativeToFloor = true;
        Raise(sim); sim.Tick(); Assert.Equal(96, actor.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(112, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void NonCorpseRaisedFloorObstructionUsesSolidityRatherThanHealth(bool dead, bool solid)
    {
        var sim = Room(64); var actor = sim.Players.Single();
        if (dead) actor.Health = 0;
        actor.Corpse = false; actor.Solid = solid;
        Raise(sim, crush: -1); LineSpecials.TickMotions(sim);
        Assert.Equal(solid ? 0 : 16, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BridgeObstructsRaisedFloorEvenWithHeadroom(bool bridge)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.ActsLikeBridge = bridge;
        Raise(sim, crush: -1); LineSpecials.TickMotions(sim);
        Assert.Equal(bridge ? 0 : 16, sim.FloorOf(0));
    }

    [Fact]
    public void RaisedFloorBelowAirborneBridgeStillMoves()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.ActsLikeBridge = true;
        actor.Z = Fixed.FromInt(64); actor.OnGround = false;
        Raise(sim, crush: -1); LineSpecials.TickMotions(sim);
        Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(64, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RaisedFloorBridgeObstructionDoesNotCrunchBridgeOrRiders(bool stop)
    {
        var sim = Room(); var bridge = sim.Players.Single(); bridge.ActsLikeBridge = true;
        var rider = sim.AddBot(0, 0); rider.Brain = null; rider.Z = bridge.Height;
        var bridgeHealth = bridge.Health; var riderHealth = rider.Health;
        Raise(sim, crush: 10, stop: stop); LineSpecials.TickMotions(sim);
        Assert.Equal(bridgeHealth, bridge.Health); Assert.Equal(riderHealth, rider.Health);
        Assert.Equal(stop ? 0 : 16, sim.FloorOf(0));
    }

    private static void Raise(AuthoritySimulation sim, int crush = 0, bool stop = false) =>
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor,
            TargetFloor = 64, FloorDirection = 1, Speed = 16, CrushDamage = crush, StopOnCrush = stop });

    private static AuthoritySimulation Room(double ceiling = 256) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = ceiling }], Things = [new LevelThing { Type = 1 }] });
}
