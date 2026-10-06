using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ReactionTimeArchiveTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void MonsterSignedTimerRoundTripsAndUpdatesAttachedBrain(int reaction)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        actor.ReactionTime = reaction;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(102, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        var restored = loaded.Actors.Single(a => a.DoomEdNum == 3004);
        Assert.Equal(reaction, restored.ReactionTime);
        Assert.Equal(reaction, restored.Brain!.ReactionTics);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
    }

    [Fact]
    public void BaselineArchiveClearsLaterRuntimeTimer()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var baseline = actor.ReactionTime; var bytes = SimSavegame.Write(sim);
        actor.ReactionTime = 99;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(baseline, actor.ReactionTime);
    }

    [Fact]
    public void PlayerAndBrainlessWakeTimersRestoreTogether()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        actor.Brain = null; actor.ReactionTime = 17;
        ActorDamage.Apply(actor, 1, flags: DamageFlags.NoPain);
        sim.Players.Single().ReactionTime = 18;
        var bytes = SimSavegame.Write(sim);
        actor.ReactionTime = 99; sim.Players.Single().ReactionTime = 99;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, actor.ReactionTime);
        Assert.Equal(18, sim.Players.Single().ReactionTime);
    }

    [Fact]
    public void InvalidPresenceMarkerIsRejected()
    {
        var sim = Room(); sim.Players.Single().ReactionTime = 18;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-reaction-time-value", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }],
    });
}
