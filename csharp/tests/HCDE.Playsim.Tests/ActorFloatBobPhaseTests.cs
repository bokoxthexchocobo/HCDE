using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFloatBobPhaseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(63)]
    public void ActionAndSavePreservePhaseIncludingExplicitZero(int phase)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetFloatBobPhase(actor, 42); ActorPropertyActions.SetFloatBobPhase(actor, phase);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FloatBobPhase = 5; sim.RestoreState(state);
        Assert.Equal(phase, actor.FloatBobPhase); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void OutOfRangeActionDoesNotChangePhaseOrSave(int phase)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.SetFloatBobPhase(actor, phase);
        Assert.Equal(0, actor.FloatBobPhase); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameActionAndDirectOverrideAffectChecksum()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var other = Room(); other.AddBot(0, 0).Brain = null;
        actor.States.Configure(actor, [new(-1, 0, Action: self => ActorPropertyActions.SetFloatBobPhase(self, 63))], 0);
        other.Actors.Single().States.Configure(other.Actors.Single(), [new(-1, 0)], 0);
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
        actor.FloatBobPhase = 30; var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FloatBobPhase = 0; sim.RestoreState(state);
        Assert.Equal(30, actor.FloatBobPhase); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 79)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    [InlineData(12, -1)]
    [InlineData(12, 64)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.SetFloatBobPhase(sim.AddBot(0, 0), 1);
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-bobphase-", error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void InvalidMemoryRejectsRestoreBeforeMutation(int phase)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors.Single().FloatBobPhase = phase; actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    [Fact]
    public void LegacyWithoutPhaseRestoresDefaultValue()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FloatBobPhase = 10; sim.RestoreState(state); Assert.Equal(0, actor.FloatBobPhase);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
