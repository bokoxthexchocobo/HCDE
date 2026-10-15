using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontFollowPlayersTests
{
    [Theory]
    [InlineData("DONTFOLLOWPLAYERS")]
    [InlineData("Actor.dontfollowplayers")]
    public void FlagSupportsSharedScriptAccess(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.DontFollowPlayers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledPlayerFollowSkipsItsNamedRandomDraws(bool dontFollow)
    {
        var baseline = Room(); var baselineActor = baseline.Actors[0]; baselineActor.Friendly = false;
        var sim = Room(); var actor = sim.Actors[0]; actor.Friendly = true; actor.DontFollowPlayers = dontFollow;
        foreach (var subject in new[] { baselineActor, actor })
        {
            subject.ReactionTime = 0; subject.OnGround = false; subject.Z = Fixed.FromInt(10);
            ActorWanderActions.Wander(subject);
        }
        Assert.Equal(dontFollow, baseline.CaptureState().NewChaseDirRandomState == sim.CaptureState().NewChaseDirRandomState);
        Assert.Equal(baselineActor.MoveDirection, actor.MoveDirection);
        Assert.Equal(baselineActor.MoveCount, actor.MoveCount);
    }

    [Fact]
    public void FlagDoesNotDisableEnemyAcquisition()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.DontFollowPlayers = true;
        Assert.Same(sim.Actors[1], FriendlyEnemySearch.Find(sim, actor));
    }

    [Fact]
    public void SavedFlagRestoresChecksumAndOlderSaveClearsIt()
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.DontFollowPlayers = true; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.DontFollowPlayers = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.DontFollowPlayers); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(actor.DontFollowPlayers); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void InvalidFlagRejectsStructuredAndBinaryArchives()
    {
        var sim = Room(); sim.Actors[0].DontFollowPlayers = true;
        var state = sim.CaptureState(); state.Actors[0].DontFollowPlayersFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-dontfollowplayers-state", error);
    }

    [Fact]
    public void RevivalRestoresRepresentedDefault()
    {
        var actor = Room().Actors[0]; actor.DontFollowPlayers = true;
        ActorRaise.ReviveSupported(actor); Assert.False(actor.DontFollowPlayers);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64, Friendly = true },
            new LevelThing { Type = 3001, X = 320, Y = 64 }, new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
