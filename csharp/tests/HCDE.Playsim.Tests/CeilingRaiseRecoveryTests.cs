using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingRaiseRecoveryTests
{
    [Theory]
    [InlineData(MotionKind.Ceiling, -8, 16, 0)]
    [InlineData(MotionKind.Door, -8, 16, 0)]
    [InlineData(MotionKind.Ceiling, -16, 16, 0)]
    [InlineData(MotionKind.Ceiling, -17, 16, -17)]
    [InlineData(MotionKind.Ceiling, -8, 8, -8)]
    [InlineData(MotionKind.Ceiling, -16, 8, -8)]
    public void BuriedRecoveryUsesOldCeilingContactAndNewHeadroom(MotionKind kind, int z, int speed, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(z);
        actor.VelocityZ = Fixed.FromInt(2);
        Raise(sim, kind, speed); LineSpecials.TickMotions(sim);
        Assert.Equal(40 + speed, sim.CeilingOf(0)); Assert.Equal(expected, actor.Z.ToDouble());
        Assert.Equal(2, actor.VelocityZ.ToDouble()); Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(true, false, false, false, false, -8)]
    [InlineData(false, true, false, false, false, -8)]
    [InlineData(false, false, true, false, false, -8)]
    [InlineData(false, false, false, true, false, -8)]
    [InlineData(false, false, false, true, true, 0)]
    public void RecoveryHonorsActorFlags(bool noLiftDrop, bool bridge, bool noSector,
        bool noBlockmap, bool moveWithSector, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(-8);
        actor.NoLiftDrop = noLiftDrop; actor.ActsLikeBridge = bridge; actor.NoSector = noSector;
        actor.NoBlockmap = noBlockmap; actor.MoveWithSector = moveWithSector;
        Raise(sim, MotionKind.Ceiling, 16); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.Equal(56, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(MotionKind.Ceiling)]
    [InlineData(MotionKind.Door)]
    public void HangingActorDoesNotFollowRisingCeiling(MotionKind kind)
    {
        var sim = Room(128); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(72);
        actor.NoGravity = actor.RelativeToFloor = true;
        Raise(sim, kind, 16); LineSpecials.TickMotions(sim);
        Assert.Equal(144, sim.CeilingOf(0)); Assert.Equal(72, actor.Z.ToDouble());
    }

    [Fact]
    public void ShortenedFinalStepUsesActualTravel()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(-17);
        Raise(sim, MotionKind.Ceiling, 16, target: 48); LineSpecials.TickMotions(sim);
        Assert.Equal(-17, actor.Z.ToDouble()); Assert.Equal(48, sim.CeilingOf(0)); Assert.Empty(sim.Motions);
    }

    [Fact]
    public void RecoveredActorSaveContinuationIsDeterministic()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(-8);
        Raise(sim, MotionKind.Ceiling, 16); LineSpecials.TickMotions(sim);
        Assert.Equal(0, actor.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
    }

    private static void Raise(AuthoritySimulation sim, MotionKind kind, int speed, double target = 256) =>
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind,
            CeilingDirection = 1, TargetCeiling = target, Speed = speed });

    private static AuthoritySimulation Room(double ceiling = 40) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = ceiling }], Things = [new LevelThing { Type = 1 }] });
}
