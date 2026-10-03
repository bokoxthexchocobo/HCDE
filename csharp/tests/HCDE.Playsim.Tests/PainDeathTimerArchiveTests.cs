using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class PainDeathTimerArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoringMidDeathSequenceRestoresRemainingDelay(bool serialized)
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); parent.Health = 0;
        for (var i = 0; i < 10; i++) sim.Tick();
        var state = sim.CaptureState();
        Assert.Equal(10, state.Actors[0].PainDeath!.Value.Tics);
        if (serialized)
        {
            var bytes = SimSavegame.Write(state); Assert.Equal(21, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        for (var i = 0; i < 10; i++) sim.Tick();
        sim.RestoreState(state);
        for (var i = 0; i < 21; i++) sim.Tick();
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == 3006);
        sim.Tick(); Assert.Equal(3, sim.Actors.Count(a => a.DoomEdNum == 3006));
    }
    [Fact]
    public void CompletedDeathSequenceDoesNotRepeatAfterRestoration()
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); parent.Health = 0;
        for (var i = 0; i < 32; i++) sim.Tick();
        var state = sim.CaptureState();
        parent.Brain!.RestorePainDeath(new(0, null));
        sim.RestoreState(state);
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.Equal(3, sim.Actors.Count(a => a.DoomEdNum == 3006));
    }
    [Theory]
    [InlineData(-2)]
    [InlineData(33)]
    public void InvalidSerializedCounterIsRejected(int tics)
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), tics);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-pain-state", error); Assert.Empty(state.Actors);
    }
    [Fact]
    public void InvalidCounterRejectsRestorationBeforeMutation()
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); var state = sim.CaptureState();
        state.Actors[0].PainDeath = new(33, null);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(400, parent.Health);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IncompatibleBrainRejectsRestoreBeforeClockOrActorMutation(bool wrongProfile)
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); var state = sim.CaptureState();
        parent.Brain = wrongProfile ? MonsterBrain.ForType(3001) : null;
        parent.Health = 123; sim.Tick(); var tic = sim.Thinkers.Clock.Tic; var x = parent.X;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(tic, sim.Thinkers.Clock.Tic); Assert.Equal(123, parent.Health); Assert.Equal(x, parent.X);
    }
    [Fact]
    public void SerializedDeathTargetRestoresChargingSoulTarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 71 }, new LevelThing { Type = 1, X = 512, Id = 17 }] });
        var parent = sim.Actors.Single(a => a.DoomEdNum == 71); var target = Assert.Single(sim.Players);
        parent.Brain!.SetTargetThingId(sim, 17); parent.Health = 0;
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        Assert.Equal(target.Id, state.Actors.Single(a => a.Id == parent.Id).PainDeath!.Value.TargetId);
        parent.Brain.RestorePainDeath(new(31, null)); sim.RestoreState(state);
        for (var i = 0; i < 22; i++) sim.Tick();
        var souls = sim.Actors.Where(a => a.DoomEdNum == 3006).ToArray(); Assert.Equal(3, souls.Length);
        Assert.All(souls, soul => { Assert.True(soul.Brain!.Charging); Assert.Equal(target.Id, soul.Brain.TargetId); });
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestroyedOrRemovedParentRejectsRestore(bool tickAfterDestroy)
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors); var state = sim.CaptureState();
        parent.Destroy(); if (tickAfterDestroy) sim.Tick();
        var tic = sim.Thinkers.Clock.Tic;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(tic, sim.Thinkers.Clock.Tic); Assert.True(parent.Destroyed);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 71 }] });
}
