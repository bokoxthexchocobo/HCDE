using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorLightSaveTests
{
    [Theory]
    [InlineData(35)]
    [InlineData(160)]
    [InlineData(0)]
    [InlineData(-32768)]
    [InlineData(32767)]
    public void CalledSpecialLightAndExplicitDefaultRestore(int value)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorSpecialActions.CallSpecial(actor, 112, 7, 35);
        ActorSpecialActions.CallSpecial(actor, 112, 7, value);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorSpecialActions.CallSpecial(actor, 112, 7, 80); sim.RestoreState(state);
        Assert.Equal(value, sim.LightOf(0)); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 80)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 32768)]
    [InlineData(8, -32769)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorSpecialActions.CallSpecial(sim.AddBot(0, 0), 112, 7, 35);
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-light-", error);
    }

    [Fact]
    public void InvalidMemoryCountRejectsBeforeActorMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Lights = [];
        actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    [Fact]
    public void LegacyAbsentLightingPreservesLiveValue()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorSpecialActions.CallSpecial(actor, 112, 7, 80); sim.RestoreState(state); Assert.Equal(80, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
