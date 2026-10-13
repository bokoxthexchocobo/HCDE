using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingRaiseOverlapTests
{
    [Theory]
    [InlineData(MotionKind.Ceiling, false)]
    [InlineData(MotionKind.Ceiling, true)]
    [InlineData(MotionKind.Door, false)]
    public void RisingCeilingRecoversPlayerOverlap(MotionKind kind, bool noLiftDrop)
    {
        var sim = Room(); var player = sim.Players.Single(); AddSupport(sim, 16);
        player.Z = Fixed.FromInt(8); player.NoLiftDrop = noLiftDrop; player.VelocityZ = Fixed.FromInt(2);
        Raise(sim, kind); LineSpecials.TickMotions(sim);
        Assert.Equal(16, player.Z.ToDouble()); Assert.Equal(2, player.VelocityZ.ToDouble());
        Assert.True(player.OnMobj); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void RecoveryCapsAtNewCeiling()
    {
        var sim = Room(64); var player = sim.Players.Single(); AddSupport(sim, 32);
        player.Z = Fixed.FromInt(8); Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(24, player.Z.ToDouble()); Assert.Equal(80, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CollisionPassThroughSuppressesRecovery(bool playerPass, bool supportPass)
    {
        var sim = Room(); var player = sim.Players.Single(); var support = AddSupport(sim, 16);
        player.Z = Fixed.FromInt(8); player.ThruActors = playerPass; support.ThruActors = supportPass;
        Raise(sim); LineSpecials.TickMotions(sim); Assert.Equal(8, player.Z.ToDouble());
    }

    [Fact]
    public void ActorAbovePlayerDoesNotBecomeSupport()
    {
        var sim = Room(); var player = sim.Players.Single(); var support = AddSupport(sim, 16);
        support.Z = Fixed.FromInt(24); player.Z = Fixed.FromInt(8);
        Raise(sim); LineSpecials.TickMotions(sim); Assert.Equal(8, player.Z.ToDouble());
    }

    [Fact]
    public void HighestOverlappingSupportWins()
    {
        var sim = Room(); var player = sim.Players.Single(); AddSupport(sim, 16); AddSupport(sim, 24);
        player.Z = Fixed.FromInt(8); Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(24, player.Z.ToDouble());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void SectorExclusionsSuppressRecovery(bool noSector, bool bridge)
    {
        var sim = Room(); var player = sim.Players.Single(); AddSupport(sim, 16);
        player.Z = Fixed.FromInt(8); player.NoSector = noSector; player.ActsLikeBridge = bridge;
        Raise(sim); LineSpecials.TickMotions(sim); Assert.Equal(8, player.Z.ToDouble());
    }

    [Fact]
    public void SaveContinuationAfterRecoveryIsDeterministic()
    {
        var sim = Room(); var player = sim.Players.Single(); AddSupport(sim, 16);
        player.Z = Fixed.FromInt(8); Raise(sim); LineSpecials.TickMotions(sim);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void NonCorpseRecoveryUsesSolidityRatherThanHealth(bool dead, bool solid)
    {
        var sim = Room(); sim.Players.Single().X = Fixed.FromInt(200);
        var actor = sim.AddBot(0, 0); actor.Brain = null;
        if (dead) actor.Health = 0;
        actor.Corpse = false; actor.Solid = solid; actor.Z = Fixed.FromInt(8);
        AddSupport(sim, 16); Raise(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(solid ? 16 : 8, actor.Z.ToDouble());
    }

    private static Actor AddSupport(AuthoritySimulation sim, int height)
    {
        var actor = sim.AddBot(0, 0); actor.Brain = null; actor.Height = Fixed.FromInt(height); return actor;
    }

    private static void Raise(AuthoritySimulation sim, MotionKind kind = MotionKind.Ceiling) =>
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind,
            CeilingDirection = 1, TargetCeiling = 256, Speed = 16 });

    private static AuthoritySimulation Room(double ceiling = 128) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = ceiling }], Things = [new LevelThing { Type = 1 }] });
}
