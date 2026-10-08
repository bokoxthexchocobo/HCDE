using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ClientStateTimingTests
{
    private sealed class ClientActor : Actor
    {
        public override bool IsClientSide() => true;
    }

    [Fact]
    public void ClientStreamMatchesNativeReferenceVector()
    {
        var sim = Room();
        uint[] expected = [4191893107u, 1033244852u, 3818332130u, 1865861872u];
        foreach (var value in expected) Assert.Equal(value, sim.NextClientStateRandom());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClientFramesUseSeparateStreamBeforeFastScaling(bool fast)
    {
        var sim = Room(); var control = Room();
        var actor = new ClientActor { Simulation = sim, AlwaysFast = fast };
        var frame = new ActorFrame(5, 0, Fast: true, TicRange: 9);
        for (var i = 0; i < 64; i++)
        {
            var tics = 5 + (int)(control.NextClientStateRandom() % 10);
            Assert.Equal(fast ? tics - (tics >> 1) : tics, frame.GetTics(actor));
        }
        Assert.Equal(Room().NextStateRandom(), sim.NextStateRandom());
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
    }

    [Fact]
    public void ClientStreamIsOmittedFromSaveAndReseededOnLoad()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim);
        var first = sim.NextClientStateRandom(); sim.NextClientStateRandom();
        Assert.Equal(saved, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, saved);
        Assert.Equal(first, sim.NextClientStateRandom());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
