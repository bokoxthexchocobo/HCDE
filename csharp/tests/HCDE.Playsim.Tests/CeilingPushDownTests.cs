using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingPushDownTests
{
    [Theory]
    [InlineData(MotionKind.Ceiling, true)]
    [InlineData(MotionKind.Door, true)]
    [InlineData(MotionKind.Ceiling, false)]
    [InlineData(MotionKind.Door, false)]
    public void ZeroHealthNonCorpseStillObstructsWhenSolid(MotionKind kind, bool solid)
    {
        var sim = Room(64); var actor = sim.Players.Single();
        actor.Health = 0; actor.Corpse = false; actor.Solid = solid;
        Lower(sim, kind); LineSpecials.TickMotions(sim);
        Assert.Equal(solid ? 64 : 48, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(MotionKind.Ceiling, -1)]
    [InlineData(MotionKind.Ceiling, 10)]
    [InlineData(MotionKind.Door, -1)]
    public void AirborneActorIsPushedDownWithoutBlockingOrCrushing(MotionKind kind, int crush)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(64);
        actor.NoGravity = true; actor.VelocityZ = Fixed.FromInt(2);
        var motion = Lower(sim, kind, crush); LineSpecials.TickMotions(sim);
        Assert.Equal(112, sim.CeilingOf(0)); Assert.Equal(56, actor.Z.ToDouble());
        Assert.Equal(100, actor.Health); Assert.Equal(2, actor.VelocityZ.ToDouble());
        Assert.False(actor.OnGround); Assert.Equal(kind == MotionKind.Door, motion.Closing);
    }

    [Theory]
    [InlineData(false, false, false, 56)]
    [InlineData(true, false, false, 64)]
    [InlineData(false, true, false, 64)]
    [InlineData(false, true, true, 56)]
    public void PushDownUsesSectorMoverEligibility(bool noSector, bool noBlockmap, bool moveWithSector, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(64);
        actor.NoSector = noSector; actor.NoBlockmap = noBlockmap; actor.MoveWithSector = moveWithSector;
        Lower(sim, MotionKind.Ceiling); LineSpecials.TickMotions(sim);
        Assert.Equal(112, sim.CeilingOf(0)); Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(MotionKind.Ceiling)]
    [InlineData(MotionKind.Door)]
    public void BridgeCannotBePushedDown(MotionKind kind)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(64); actor.ActsLikeBridge = true;
        Lower(sim, kind); LineSpecials.TickMotions(sim);
        Assert.Equal(128, sim.CeilingOf(0)); Assert.Equal(64, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(-1, 64, 100)]
    [InlineData(10, 48, 90)]
    public void InsufficientHeadroomStillObstructsOrCrushes(int crush, int expectedCeiling, int health)
    {
        var sim = Room(64); var actor = sim.Players.Single();
        Lower(sim, MotionKind.Ceiling, crush); LineSpecials.TickMotions(sim);
        Assert.Equal(expectedCeiling, sim.CeilingOf(0)); Assert.Equal(health, actor.Health);
        Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void ExactHeadroomPushesActorToFloorWithoutDamage()
    {
        var sim = Room(72); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(8);
        Lower(sim, MotionKind.Ceiling, 10); LineSpecials.TickMotions(sim);
        Assert.Equal(56, sim.CeilingOf(0)); Assert.Equal(0, actor.Z.ToDouble());
        Assert.True(actor.OnGround); Assert.Equal(100, actor.Health);
    }

    [Fact]
    public void SaveRestoreContinuesCeilingPushDownExactly()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(64);
        actor.OnGround = false; actor.NoGravity = true;
        Lower(sim, MotionKind.Ceiling); sim.Tick(); Assert.Equal(56, actor.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(40, actor.Z.ToDouble());
    }

    private static SectorMotion Lower(AuthoritySimulation sim, MotionKind kind, int crush = -1)
    {
        var motion = new SectorMotion { SectorIndex = 0, Kind = kind, CeilingDirection = -1,
            TargetCeiling = 0, ClosedCeiling = 0, Closing = kind == MotionKind.Door, Speed = 16, CrushDamage = crush };
        sim.Motions.Add(motion); return motion;
    }

    private static AuthoritySimulation Room(double ceiling = 128) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = ceiling }], Things = [new LevelThing { Type = 1 }] });
}
