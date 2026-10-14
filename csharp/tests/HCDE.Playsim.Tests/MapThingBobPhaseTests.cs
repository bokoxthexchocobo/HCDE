using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingBobPhaseTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(42, 42)]
    [InlineData(63, 63)]
    [InlineData(-1, 0)]
    [InlineData(64, 0)]
    [InlineData(255, 0)]
    public void DefaultStartupAppliesOnlyNativeMapPhaseRange(int phase, int expected)
    {
        var sim = AuthoritySimulation.Start(Level(phase));
        Assert.Equal(expected, sim.Actors.Single().FloatBobPhase);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(63, 63)]
    [InlineData(-1, 200)]
    [InlineData(64, 200)]
    public void MapOverrideFollowsRandomInitializationWithoutSkippingDraws(int phase, int expected)
    {
        var values = new Queue<uint>([17, 200]);
        var actor = ActorSpawner.Spawn(Level(phase), new ThinkerCollection(), actorSpawnRandom: values.Dequeue).Single();
        Assert.Equal(17, actor.LastLookPlayerNumber); Assert.Equal(expected, actor.FloatBobPhase); Assert.Empty(values);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    public void MapPhaseSurvivesIdentityReconstructionAndRuntimeChanges(int phase)
    {
        var sim = AuthoritySimulation.Start(Level(phase)); var actor = sim.Actors.Single();
        actor.FloatBobPhase = 200; var saved = SimSavegame.Write(sim, true);
        actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(200, sim.Actors.Single(a => !a.Destroyed).FloatBobPhase);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ParsedMapPropertyReachesRuntimeActor()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 3004; floatbobphase = 42; skill3 = true; single = true; }", out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        Assert.Equal(42, sim.Actors.Single().FloatBobPhase);
    }

    private static PlayLevel Level(int phase) => new()
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, FloatBobPhase = phase }]
    };
}
