using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeeFriendlyMonstersTests
{
    [Theory]
    [InlineData("SEEFRIENDLYMONSTERS")]
    [InlineData("Actor.seefriendlymonsters")]
    public void FlagSupportsScriptGetAndSet(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.SeeFriendlyMonsters);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HostileMonsterSearchIsGatedByFlag(bool enabled)
    {
        var sim = Room(); var observer = sim.Actors[0]; var friendly = sim.Actors[1];
        observer.SeeFriendlyMonsters = enabled;
        Assert.Equal(enabled ? friendly : null, FriendlyEnemySearch.Find(sim, observer));
        observer.ReactionTime = 0; observer.Brain!.Tick(sim, observer);
        Assert.Equal(enabled ? friendly.Id : sim.Players.Single().Id, observer.TargetId);
    }

    [Fact]
    public void FlagAndChecksumRestoreAndOldSaveClearsFlag()
    {
        var sim = Room(); var observer = sim.Actors[0]; var old = SimSavegame.Write(sim, true);
        var oldHash = sim.Checksum;
        observer.SeeFriendlyMonsters = true; sim.RestoreState(sim.CaptureState());
        var saved = SimSavegame.Write(sim, true); var hash = sim.Checksum;
        Assert.NotEqual(oldHash, hash);
        observer.SeeFriendlyMonsters = false; SimSavegame.Apply(sim, saved);
        Assert.True(observer.SeeFriendlyMonsters); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(observer.SeeFriendlyMonsters); Assert.Equal(oldHash, sim.Checksum);
    }

    [Fact]
    public void InvalidSavedFlagRejectsReadAndRestore()
    {
        var sim = Room(); sim.Actors[0].SeeFriendlyMonsters = true;
        var state = sim.CaptureState(); state.Actors[0].SeeFriendlyMonstersFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-seefriendlymonsters-state", error);
    }

    [Fact]
    public void RevivalRestoresRepresentedClassDefault()
    {
        var actor = Room().Actors[0]; actor.SeeFriendlyMonsters = true;
        ActorRaise.ReviveSupported(actor);
        Assert.False(actor.SeeFriendlyMonsters);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64 },
            new LevelThing { Type = 3001, X = 320, Y = 64, Friendly = true },
            new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
