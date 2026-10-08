using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeDeathActionTests
{
    [Fact]
    public void StateActionUsesTwoNativeBytesAndSetsRepresentedFlags()
    {
        var sim = Room(); var control = Room(); var actor = sim.Actors.Single();
        var random = NativeStateRandom.Seed(42, 0x9ee1e1b9u);
        for (var i = 0; i < 16; i++)
        {
            var expected = 75 + (int)(NativeStateRandom.Next(ref random) & 255)
                + (int)(NativeStateRandom.Next(ref random) & 255);
            actor.Solid = actor.Shootable = actor.IceCorpse = actor.Pushable = false;
            actor.States.Configure(actor, [new ActorFrame(1, -1, ActorFreezeActions.FreezeDeath)], 0);
            Assert.Equal(expected, actor.States.RemainingTics);
            Assert.True(actor.Solid && actor.Shootable && actor.IceCorpse && actor.Pushable);
        }
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        Assert.Equal(control.NextIceTics(), sim.NextIceTics());
        Assert.Equal(control.NextFreezeChunkByte(), sim.NextFreezeChunkByte());
    }

    [Fact]
    public void SaveRestoresTimerFlagsAndFullStreamAndOlderSaveReseeds()
    {
        var sim = Room(); var older = SimSavegame.Write(sim); var actor = sim.Actors.Single();
        actor.States.Configure(actor, [new ActorFrame(1, -1)], 0);
        ActorFreezeActions.FreezeDeath(actor); var saved = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        Assert.True(state.FreezeDeathRandomState > uint.MaxValue);
        var restored = Room(); restored.Actors.Single().States.Configure(restored.Actors.Single(), [new ActorFrame(1, -1)], 0);
        SimSavegame.Apply(restored, saved);
        Assert.Equal(actor.States.RemainingTics, restored.Actors.Single().States.RemainingTics);
        Assert.True(restored.Actors.Single().IceCorpse && restored.Actors.Single().Pushable);
        Assert.Equal(saved, SimSavegame.Write(restored));
        Assert.Equal(sim.NextFreezeDeathTics(), restored.NextFreezeDeathTics());
        SimSavegame.Apply(sim, older);
        Assert.Equal(Room().NextFreezeDeathTics(), sim.NextFreezeDeathTics());
    }

    [Fact]
    public void DetachedActionFailsBeforeChangingFlags()
    {
        var actor = new Actor { Solid = false, Shootable = false };
        Assert.Throws<InvalidOperationException>(() => ActorFreezeActions.FreezeDeath(actor));
        Assert.False(actor.Solid || actor.Shootable || actor.IceCorpse || actor.Pushable);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedStreamArchiveFails(bool header)
    {
        var sim = Room(); sim.NextFreezeDeathTics(); var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(header ? saved.Length - size : saved.Length - 4), header ? 117 : 15);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-freezedeathrandom-header" : "save-freezedeathrandom-size", error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedActorFlagsOrCountFail(bool count)
    {
        var sim = Room(); sim.NextFreezeDeathTics(); var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + (count ? 12 : 16)), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(count ? "save-freezedeathrandom-size" : "save-freezedeathrandom-flags", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
