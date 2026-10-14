using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDieRandomTests
{
    [Theory]
    [InlineData(0, 6, 4, 6, 3)]
    [InlineData(1, 4, 6, 3, 3)]
    [InlineData(12345, 5, 4, 4, 5)]
    public void NativeStreamVectorsControlDeathFrameTiming(int seed, int a, int b, int c, int d)
    {
        var sim = Room(seed);
        Assert.Equal(new[] { a, b, c, d }, Enumerable.Range(0, 4).Select(_ => Kill(sim).States.RemainingTics));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void HeldAndOneTicDeathFramesDoNotConsumeRandomness(int tics)
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(tics, 1)], 0); actor.DeathState = 1;
        ActorDamage.Apply(actor, 1000);
        Assert.Equal(tics, actor.States.RemainingTics); Assert.Null(sim.CaptureState().ActorDieRandomState);
        Assert.Equal(4, Kill(sim).States.RemainingTics);
    }

    [Fact]
    public void ShortFramesClampToOneTic()
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(2, 1)], 0); actor.DeathState = 1;
        ActorDamage.Apply(actor, 1000); Assert.Equal(1, actor.States.RemainingTics);
    }

    [Fact]
    public void StateActionTimingIsUsedAfterEntry()
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(6, 1, a => a.States.SetTics(10))], 0); actor.DeathState = 1;
        ActorDamage.Apply(actor, 1000); Assert.Equal(8, actor.States.RemainingTics);
    }

    [Fact]
    public void DirectHealthAssignmentDoesNotUseDieStream()
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.Health = 0;
        Assert.Equal(6, actor.States.RemainingTics); Assert.Null(sim.CaptureState().ActorDieRandomState);
    }

    [Fact]
    public void PuffRandomnessDoesNotAdvanceActorDieStream()
    {
        var sim = Room(1); sim.SpawnHitscanPuff(0, 0, 20);
        Assert.Equal(4, Kill(sim).States.RemainingTics);
    }

    [Fact]
    public void SaveRestoresStreamAndLegacySaveResetsIt()
    {
        var sim = Room(1); var legacy = SimSavegame.Write(sim, true);
        Kill(sim); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(227, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        Assert.Equal(6, Kill(sim).States.RemainingTics);
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
        Assert.Equal(6, Kill(sim).States.RemainingTics);
        SimSavegame.Apply(sim, legacy); Assert.Null(sim.CaptureState().ActorDieRandomState);
        Assert.Equal(4, Kill(sim).States.RemainingTics);
    }

    [Fact]
    public void InvalidArchivePredecessorIsRejected()
    {
        var sim = Room(1); Kill(sim); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 227);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-actordierandom-header", error);
    }

    [Fact]
    public void MissingDeathStateDoesNotConsumeRandomness()
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.DeathState = -1;
        ActorDamage.Apply(actor, 1000); Assert.True(actor.Destroyed);
        Assert.Null(sim.CaptureState().ActorDieRandomState);
    }

    [Fact]
    public void ActionThatHoldsDeathFrameDoesNotConsumeRandomness()
    {
        var sim = Room(1); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(6, 1, a => a.States.SetTics(-1))], 0); actor.DeathState = 1;
        ActorDamage.Apply(actor, 1000); Assert.Equal(-1, actor.States.RemainingTics);
        Assert.Null(sim.CaptureState().ActorDieRandomState);
    }

    [Fact]
    public void RandomMetadataWithoutGeometryRejectsWriteAndRestore()
    {
        var sim = Room(1); var state = sim.CaptureState(); state.ActorDieRandomState = 42; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Null(sim.CaptureState().ActorDieRandomState);
    }

    [Fact]
    public void StreamChangesParticipateInChecksum()
    {
        var sim = Room(1); var control = Room(1); sim.NextActorDieRandom();
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    private static Actor Kill(AuthoritySimulation sim)
    {
        var actor = sim.AddBot(200, 0); actor.Brain = null; ActorDamage.Apply(actor, 1000); return actor;
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    }, rngSeed: seed);
}
