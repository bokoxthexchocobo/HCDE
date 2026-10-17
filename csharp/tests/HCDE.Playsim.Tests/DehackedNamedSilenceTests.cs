using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNamedSilenceTests
{
    public static IEnumerable<object[]> Cases => new[] {
        ("Alert", ActorSoundType.See), ("Attack", ActorSoundType.Attack), ("Pain", ActorSoundType.Pain),
        ("Death", ActorSoundType.Death), ("Action", ActorSoundType.Active), ("Rip", ActorSoundType.Rip),
    }.SelectMany(slot => new[] { "None", "none", "NONE", "", "  ", "\t" }.Select(value => new object[] { slot.Item1, slot.Item2, value }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void SilentValuesRemainExplicitThroughLayeringMapDropAndFreshSave(string field, ActorSoundType slot, string value)
    {
        var original = DehackedPatch.Apply($"Thing 64\n{field} sound = custom/original\n");
        var silent = DehackedPatch.Apply($"Thing 64\n{field} sound = {value}\n", original);
        var patch = DehackedPatch.Apply("Thing 64\nMass = 240\n", silent); Assert.Empty(patch.Errors);
        Assert.Equal("custom/original", original.Actors.Single(a => a.Index == 64).SoundNameOverrides[(int)slot]);
        Assert.Equal("", patch.Actors.Single(a => a.Index == 64).SoundNameOverrides[(int)slot]);
        var sim = Room(patch); Assert.Equal("", sim.Actors.Single(a => a.MapPickupThingIndex.HasValue).ActorSounds[slot]);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup); Assert.Equal("", drop.ActorSounds[slot]);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == drop.Id); Assert.Equal("", loaded.ActorSounds[slot]);
        switch (slot)
        {
            case ActorSoundType.See: ActorSoundActions.SeeSound(loaded); break;
            case ActorSoundType.Pain: ActorSoundActions.Pain(loaded); break;
            case ActorSoundType.Death: ActorSoundActions.Scream(loaded); break;
            case ActorSoundType.Active: ActorSoundActions.PlayActiveSound(loaded); break;
            case ActorSoundType.Rip:
                loaded.Damage = 1; fresh.Players.Single().NoPain = true;
                ActorMissileDamage.Apply(loaded, fresh.Players.Single(), ripper: true); break;
        }
        Assert.Empty(fresh.DrainSoundRequests());
        var named = DehackedPatch.Apply($"Thing 64\n{field} sound = custom/replacement\n", patch);
        Assert.Equal("custom/replacement", named.Actors.Single(a => a.Index == 64).SoundNameOverrides[(int)slot]);
    }

    [Fact]
    public void EmptyProjectileSoundsSuppressStockSpawnAndDeathAfterFreshRestore()
    {
        var patch = DehackedPatch.Apply("Thing 34\nAlert sound =\nDeath sound =   \n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal("", missile.ActorSounds[ActorSoundType.See]); Assert.Equal("", missile.ActorSounds[ActorSoundType.Death]);
        ActorSoundActions.Scream(missile); Assert.Empty(sim.DrainSoundRequests());
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        var loaded = fresh.Actors.OfType<ProjectileActor>().Single();
        ActorSoundActions.PlaySpawnSound(loaded); ActorSoundActions.Scream(loaded);
        Assert.Empty(fresh.DrainSoundRequests()); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
