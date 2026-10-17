using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CustomCrushStateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CustomCrushOverridesGenericAndWorksWithoutBlood(bool noBlood)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.CrushState = actor.SpawnState; actor.NoBlood = noBlood;
        ActorPhysics.GrindActor(actor);
        Assert.Equal(actor.CrushState, actor.States.Current);
        Assert.True(actor.DontGib); Assert.False(actor.Solid); Assert.Equal(0, actor.Height.Raw);
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void InvalidCustomIndexFallsBackToRepresentedGenericState()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.CrushState = int.MaxValue;
        ActorPhysics.GrindActor(actor);
        Assert.Equal(actor.GenericCrushState, actor.States.Current);
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound == "misc/fallingsplat");
    }

    [Fact]
    public void PlayerCorpseCanUseCustomCrushStateWithoutGenericSound()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); ActorDamage.Apply(player, 1000); sim.DrainSoundRequests();
        player.CrushState = player.SpawnState; player.NoBlood = true;
        ActorPhysics.GrindActor(player);
        Assert.True(player.DontGib); Assert.Equal(0, player.Height.Raw); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void NamedStateSaveChecksumAndOldResetAreSupported()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); var older = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        actor.CrushState = actor.SpawnState;
        Assert.True(AcsActorStates.TryFindNamedState(actor, "Crush", true, out var state));
        Assert.Equal(actor.CrushState, state);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(308, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = Assert.Single(restored.Actors);
        Assert.Equal(actor.CrushState, loaded.CrushState); Assert.Equal(sim.Checksum, restored.Checksum);
        ActorPhysics.GrindActor(loaded); Assert.Equal(loaded.CrushState, loaded.States.Current);
        SimSavegame.Apply(restored, older); Assert.Equal(-1, loaded.CrushState);
    }

    [Fact]
    public void InvalidNegativeBinaryIndexIsRejected()
    {
        var sim = Room(); Assert.Single(sim.Actors).CrushState = 0;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), -2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 18 }] });
        var actor = Assert.Single(sim.Actors); actor.Health = 0; actor.Corpse = true;
        return sim;
    }
}
