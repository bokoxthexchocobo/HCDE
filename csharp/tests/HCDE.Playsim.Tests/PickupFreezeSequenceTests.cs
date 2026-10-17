using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupFreezeSequenceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IceDeathUsesInheritedSequenceOnlyForMonsterInventory(bool monster)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL+SHOOTABLE{(monster ? "+COUNTKILL" : "")}\nHit points = 10\n");
        Assert.Empty(patch.Errors); var sim = Room(patch); var actor = Item(sim);
        Assert.True(AcsActorStates.TryFindNamedState(actor, "GenericFreezeDeath", true, out var state));
        Assert.Equal(actor.GenericFreezeDeath, state);
        ActorDamage.Apply(actor, 20, damageType: "Ice");
        if (!monster) { Assert.True(actor.Destroyed); return; }
        Assert.False(actor.Destroyed); Assert.True(actor.IceCorpse); Assert.True(actor.Solid); Assert.True(actor.Shootable);
        Assert.Equal(state, actor.States.Current); Assert.InRange(actor.States.RemainingTics, 1, 585);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.Sound == "misc/freeze");
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = Item(fresh); actor.States.ForceRemainingTics(1); loaded.States.ForceRemainingTics(1);
        actor.VelocityX = actor.VelocityY = actor.VelocityZ = default;
        loaded.VelocityX = loaded.VelocityY = loaded.VelocityZ = default;
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
        Assert.Contains(fresh.Actors, a => a is IceChunkActor);
        sim.Tick(); fresh.Tick(); Assert.True(actor.Destroyed); Assert.True(loaded.Destroyed);
        Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Fact]
    public void MovingFrozenPickupWaitsBeforeShattering()
    {
        var sim = Room(DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SHOOTABLE+COUNTKILL\nHit points = 10\n"));
        var actor = Item(sim); ActorDamage.Apply(actor, 20, damageType: "Ice");
        actor.VelocityX = Fixed.FromInt(1); actor.States.ForceRemainingTics(1);
        sim.Tick(); Assert.False(actor.Destroyed); Assert.Equal(105, actor.States.RemainingTics);
        Assert.DoesNotContain(sim.Actors, a => a is IceChunkActor);
    }

    [Fact]
    public void FrozenDroppedInventoryReconstructsAndReplays()
    {
        var patch = DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SHOOTABLE+COUNTKILL\nHit points = 10\n");
        var sim = Room(patch); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup); drop.SetXYZ(200, 0, 0);
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        ActorDamage.Apply(drop, 20, damageType: "Ice"); Assert.True(drop.IceCorpse);
        var saved = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void StealthPickupIceDeathStopsDamageTriggeredFadeAcrossRestore()
    {
        var patch = DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SHOOTABLE+COUNTKILL+STEALTH\nHit points = 10\n");
        Assert.Empty(patch.Errors); var sim = Room(patch); var actor = Item(sim);
        actor.VisibilityDirection = -1; actor.Alpha = 0.25;
        ActorDamage.Apply(actor, 20, damageType: "Ice");
        Assert.True(actor.IceCorpse); Assert.True(actor.Stealth);
        Assert.Equal(0, actor.VisibilityDirection); Assert.Equal(1, actor.Alpha);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
        Assert.Equal(1, Item(fresh).Alpha); Assert.Equal(0, Item(fresh).VisibilityDirection);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
