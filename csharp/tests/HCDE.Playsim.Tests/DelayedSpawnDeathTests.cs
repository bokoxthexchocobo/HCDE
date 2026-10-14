using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DelayedSpawnDeathTests
{
    [Theory]
    [InlineData(3004, 20)]
    [InlineData(3001, 60)]
    [InlineData(3005, 400)]
    public void NativeSpawnHealthRemainsAvailableAndDeathRunsOnce(int type, int spawnHealth)
    {
        var sim = Room(type); var actor = Target(sim);
        Assert.Equal(0, actor.Health); Assert.Equal(spawnHealth, actor.SpawnHealth());
        Assert.True(actor.PendingSpawnDeath); Assert.False(actor.Killed); Assert.Equal(0, actor.DeathCount);
        sim.Tick();
        Assert.False(actor.PendingSpawnDeath); Assert.True(actor.Killed); Assert.Equal(1, actor.DeathCount);
        Assert.Equal(0, actor.Health); Assert.False(actor.Shootable); Assert.Null(actor.LastDamageSourceId);
        Assert.Equal(6, actor.States.RemainingTics);
        var count = sim.Actors.Count;
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(1, actor.DeathCount); Assert.Equal(count, sim.Actors.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SaveBeforeFirstTickReplaysPendingDeath(bool reconstruct)
    {
        var sim = Room(); var actor = Target(sim); var before = SimSavegame.Write(sim, true);
        Assert.Equal(226, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        if (reconstruct) { actor.Destroy(); sim.Tick(); }
        SimSavegame.Apply(sim, before);
        Assert.True(Target(sim).PendingSpawnDeath); Assert.Equal(0, Target(sim).DeathCount);
        Assert.Equal(before, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void OrdinarySaveRestoresPendingDeathWithoutReconstruction()
    {
        var sim = Room(); var before = SimSavegame.Write(sim); sim.Tick();
        SimSavegame.Apply(sim, before); Assert.True(Target(sim).PendingSpawnDeath);
        sim.Tick(); Assert.False(Target(sim).PendingSpawnDeath);
    }

    [Fact]
    public void ChangingStateCancelsPendingSpawnDeath()
    {
        var sim = Room(); var actor = Target(sim); actor.States.Enter(actor, ActorStateMachine.Corpse);
        Assert.False(actor.PendingSpawnDeath); sim.Tick(); Assert.Equal(0, actor.DeathCount);
    }

    [Fact]
    public void InvalidStateDoesNotCancelPendingSpawnDeath()
    {
        var sim = Room(); var actor = Target(sim);
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.States.Enter(actor, int.MaxValue));
        Assert.True(actor.PendingSpawnDeath); sim.Tick(); Assert.Equal(1, actor.DeathCount);
    }

    [Theory]
    [InlineData(0, 225, "save-spawndeath-header")]
    [InlineData(4, -1, "save-spawndeath-header")]
    [InlineData(8, 2, "save-spawndeath-value")]
    public void InvalidPendingArchiveIsRejected(int offset, int value, string expected)
    {
        var bytes = SimSavegame.Write(Room(), true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void UnsupportedPlayerPendingStateRejectsRestoreBeforeMutation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        var before = SimSavegame.Write(sim, true); var state = sim.CaptureState(true); state.Actors.Single().SpawnDeathFlag = 1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ZeroHealthPlayerStartStillReportsUnsupportedPath()
    {
        Assert.Throws<NotSupportedException>(() => AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1, Health = 0 }] }));
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapMonsterThingIndex.HasValue);
    private static AuthoritySimulation Room(int type = 3004) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type, Health = 0 }] });
}
