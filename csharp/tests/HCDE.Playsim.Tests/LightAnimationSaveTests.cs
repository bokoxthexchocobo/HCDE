using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightAnimationSaveTests
{
    [Theory]
    [InlineData(113)]
    [InlineData(114)]
    [InlineData(115)]
    [InlineData(116)]
    [InlineData(232)]
    public void SpecialAnimationsReplayExactlyAfterRestore(int special)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        ActorSpecialActions.CallSpecial(actor, special, 7, 200, 40, 8, 5);
        VerifyReplay(sim);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(17)]
    [InlineData(21)]
    public void MapAnimationsReplayExactlyAfterRestore(int special) => VerifyReplay(Room(special));

    [Fact]
    public void StoppedAnimationsRestoreEmptyList()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        ActorSpecialActions.CallSpecial(actor, 115, 7, 200, 40);
        ActorSpecialActions.CallSpecial(actor, 117, 7);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorSpecialActions.CallSpecial(actor, 115, 7, 200, 40); sim.RestoreState(state);
        Assert.Empty(sim.LightEffects); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 81)]
    [InlineData(4, int.MaxValue)]
    [InlineData(24, -1)]
    [InlineData(28, 8)]
    [InlineData(32, 32768)]
    public void MalformedAnimationIsRejected(int offset, int value)
    {
        var sim = Room(1); var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-lightanimation-", error);
    }

    [Fact]
    public void InvalidMemoryRejectsBeforeActorMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Lights = [160];
        state.LightAnimation = new(0, 0, 0, 0, [new(0, 0, 160, 40, 0, 0, 0)]); actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    private static void VerifyReplay(AuthoritySimulation sim)
    {
        for (var i = 0; i < 3; i++) sim.Tick();
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var expected = new List<(short Light, uint Hash)>();
        for (var i = 0; i < 100; i++) { sim.Tick(); expected.Add((sim.LightOf(0), sim.Checksum)); }
        sim.RestoreState(state); Assert.Equal(bytes, SimSavegame.Write(sim));
        foreach (var step in expected) { sim.Tick(); Assert.Equal(step.Light, sim.LightOf(0)); Assert.Equal(step.Hash, sim.Checksum); }
    }

    private static AuthoritySimulation Room(int special = 0) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160, Special = (short)special }] });
}
