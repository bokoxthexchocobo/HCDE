using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryDebrisRipSoundDefaultsTests
{
    [Theory]
    [InlineData("", "misc/ripslop")]
    [InlineData("Mass = 240", "misc/ripslop")]
    [InlineData("Pain sound = custom/pain", "misc/ripslop")]
    [InlineData("Rip sound = 0", "")]
    [InlineData("Rip sound = custom/rip", "custom/rip")]
    public void PickupInheritsRipSoundUnlessExplicitlyReplaced(string body, string expected)
    {
        var patch = DehackedPatch.Apply(body.Length == 0 ? "" : $"Thing 64\n{body}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(expected, map.ActorSounds[ActorSoundType.Rip]);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(expected, drop.ActorSounds[ActorSoundType.Rip]); drop.Damage = 1;
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == drop.Id);
        var player = fresh.Players.Single(); player.NoPain = true;
        ActorMissileDamage.Apply(loaded, player, ripper: true);
        Assert.True(player.Health < 100);
        if (expected.Length == 0) Assert.Empty(fresh.DrainSoundRequests());
        else Assert.Contains(fresh.DrainSoundRequests(), sound => sound.Sound == expected && sound.Channel == 4);
    }

    [Fact]
    public void DebrisRipSoundReconstructsFromFreshSave()
    {
        var sim = Room(DehackedPatch.Apply("")); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level });
        Assert.All(sim.Actors.OfType<IceChunkActor>(), chunk => Assert.Equal("misc/ripslop", chunk.ActorSounds[ActorSoundType.Rip]));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(DehackedPatch.Apply("")); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.All(fresh.Actors.OfType<IceChunkActor>(), chunk => Assert.Equal("misc/ripslop", chunk.ActorSounds[ActorSoundType.Rip]));
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData("", "misc/ripslop")]
    [InlineData("Rip sound = custom/rip", "custom/rip")]
    [InlineData("Rip sound = 0", "")]
    public void SaveWithoutSoundDataFallsBackToFactoryDefaults(string body, string expected)
    {
        var patch = DehackedPatch.Apply(body.Length == 0 ? "" : $"Thing 64\n{body}\n");
        var sim = Room(patch); var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.ActorSounds = new Dictionary<ActorSoundType, string>();
        var saved = SimSavegame.Write(state); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, fresh.Actors.Single(a => a.MapPickupThingIndex.HasValue).ActorSounds[ActorSoundType.Rip]);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
