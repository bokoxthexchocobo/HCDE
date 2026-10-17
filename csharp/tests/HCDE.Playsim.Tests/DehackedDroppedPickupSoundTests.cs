using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDroppedPickupSoundTests
{
    [Theory]
    [InlineData("Alert", ActorSoundType.See)]
    [InlineData("Attack", ActorSoundType.Attack)]
    [InlineData("Pain", ActorSoundType.Pain)]
    [InlineData("Death", ActorSoundType.Death)]
    [InlineData("Action", ActorSoundType.Active)]
    [InlineData("Rip", ActorSoundType.Rip)]
    public void AllPatchedSoundSelectorsReachDropsAndFreshReconstruction(string field, ActorSoundType slot)
    {
        var patch = DehackedPatch.Apply($"Thing 64\n{field} sound = custom/drop\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); Assert.Equal("custom/drop", drop.ActorSounds[slot]);
        Assert.Empty(sim.DrainSoundRequests());
        var saved = SimSavegame.Write(sim, true);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal("custom/drop", Drop(sim).ActorSounds[slot]); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal("custom/drop", Drop(fresh).ActorSounds[slot]); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData("1", "weapons/pistol")]
    [InlineData("500", "dehextra/sound000")]
    [InlineData("0", "")]
    public void NumericAndSilentDeathSoundsEmitAfterFreshRestore(string value, string expected)
    {
        var patch = DehackedPatch.Apply($"Thing 78\nDeath sound = {value}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Shotgun));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        var drop = Drop(fresh); Assert.Equal(expected, drop.ActorSounds[ActorSoundType.Death]);
        ActorSoundActions.Scream(drop);
        if (expected.Length == 0) Assert.Empty(fresh.DrainSoundRequests());
        else { var sound = Assert.Single(fresh.DrainSoundRequests()); Assert.Equal(expected, sound.Sound); Assert.Equal(1, sound.Attenuation); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullVolumeClassDefaultsAffectSeeAndDeathAfterRestore(bool fullVolume)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL{(fullVolume ? "+FULLVOLSOUNDS" : "")}\nAlert sound = custom/see\nDeath sound = custom/death\n");
        Assert.Empty(patch.Errors); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        var drop = Drop(fresh); Assert.Equal(fullVolume, drop.FullVolumeSee); Assert.Equal(fullVolume, drop.FullVolumeDeath);
        Assert.Equal(fullVolume, drop.DefaultFullVolumeSee); Assert.Equal(fullVolume, drop.DefaultFullVolumeDeath);
        ActorSoundActions.SeeSound(drop); Assert.Equal(fullVolume ? 0 : 1, Assert.Single(fresh.DrainSoundRequests()).Attenuation);
        ActorSoundActions.Scream(drop); Assert.Equal(fullVolume ? 0 : 1, Assert.Single(fresh.DrainSoundRequests()).Attenuation);
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, compat: CompatSurface.NoTossDrops);
}
