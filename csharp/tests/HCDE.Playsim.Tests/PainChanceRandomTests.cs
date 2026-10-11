using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PainChanceRandomTests
{
    [Fact]
    public void PainUsesNativeNamedSequenceAndLeavesCombatAlone()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.Brain = null; actor.PainChance = 128;
        var random = NativeStateRandom.Seed(42, 0xdae49b5au);
        var expected = (NativeStateRandom.Next(ref random) & 255) < 128;
        var combat = sim.CombatRandomState;
        Assert.Equal(expected, actor.TriggerPainChance());
        Assert.Equal(random, sim.CaptureState().PainChanceRandomState);
        Assert.Equal(combat, sim.CombatRandomState);
    }

    [Fact]
    public void SaveRestoresNextPainDrawAndLegacyResetsStream()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.NextPainChanceByte();
        var bytes = SimSavegame.Write(sim); var next = sim.NextPainChanceByte();
        SimSavegame.Apply(sim, bytes); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(next, sim.NextPainChanceByte());
        SimSavegame.Apply(sim, legacy); Assert.Null(sim.CaptureState().PainChanceRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: 42);
}
