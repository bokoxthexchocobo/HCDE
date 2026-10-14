using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class SplashAlertTests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void OnlyFlaggedSplashListenersAreSkipped(bool splash, bool noSplash, bool hears)
    {
        var (sim, emitter, actor) = Room(); actor.NoSplashAlert = noSplash; actor.LastHeardTargetId = actor.Id;
        SoundPropagation.Alert(sim, emitter, emitter, splash: splash);
        Assert.Equal(hears ? emitter.Id : actor.Id, actor.LastHeardTargetId);
        Assert.Equal(hears ? emitter.Id : (uint?)null, actor.TargetId);
    }

    [Fact]
    public void AcsFlagSupportsPrefixAndCase()
    {
        var (_, _, actor) = Room(); Assert.True(AcsActorFlags.TrySet(actor, "actor.nosplashalert", true));
        Assert.True(AcsActorFlags.TryGet(actor, "NOSPLASHALERT", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "NOSPLASHALERT", false)); Assert.False(actor.NoSplashAlert);
    }

    [Fact]
    public void SavedFlagSuppressesSplashAndOlderSaveClearsIt()
    {
        var (sim, emitter, actor) = Room(); var older = SimSavegame.Write(sim, true);
        actor.NoSplashAlert = true; var saved = SimSavegame.Write(sim, true);
        actor.NoSplashAlert = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.NoSplashAlert); Assert.Equal(saved, SimSavegame.Write(sim, true));
        SoundPropagation.Alert(sim, emitter, emitter, splash: true); Assert.Null(actor.LastHeardTargetId);
        SimSavegame.Apply(sim, older); Assert.False(actor.NoSplashAlert);
        SoundPropagation.Alert(sim, emitter, emitter, splash: true); Assert.Equal(emitter.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void InvalidSavedFlagIsRejected()
    {
        var (sim, _, actor) = Room(); actor.NoSplashAlert = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static (AuthoritySimulation Sim, Actor Emitter, Actor Listener) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }] });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.ClassDoomEdNum == 3004));
    }
}
