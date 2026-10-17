using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFullVolumeDefaultsTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, 34, true)]
    [InlineData(ProjectileKind.Rocket, 34, false)]
    [InlineData(ProjectileKind.Plasma, 35, true)]
    [InlineData(ProjectileKind.Plasma, 35, false)]
    [InlineData(ProjectileKind.RevenantTracer, 7, true)]
    [InlineData(ProjectileKind.RevenantTracer, 7, false)]
    public void DefaultsControlSoundAttenuationAndReconstructFromSave(ProjectileKind kind, int thing, bool fullVolume)
    {
        var patch = DehackedPatch.Apply($"Thing {thing}\nBits = {(fullVolume ? "FULLVOLSOUNDS+NOGRAVITY" : "NOGRAVITY")}\n");
        var (sim, missile) = Room(patch, kind); var stockSounds = missile.ActorSounds.ToDictionary();
        Assert.Equal(fullVolume, missile.FullVolumeDeath); Assert.Equal(fullVolume, missile.FullVolumeSee);
        Assert.Equal(fullVolume, missile.DefaultFullVolumeDeath); Assert.Equal(fullVolume, missile.DefaultFullVolumeSee);
        sim.DrainSoundRequests(); ActorSoundActions.Scream(missile);
        Assert.Equal(fullVolume ? 0 : 1, Assert.Single(sim.DrainSoundRequests()).Attenuation);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch, kind); fresh.DrainSoundRequests();
        SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(stockSounds.OrderBy(p => p.Key), loaded.ActorSounds.OrderBy(p => p.Key));
        loaded.FullVolumeDeath = loaded.FullVolumeSee = !fullVolume; ActorRaise.ReviveSupported(loaded);
        Assert.Equal(fullVolume, loaded.FullVolumeDeath); Assert.Equal(fullVolume, loaded.FullVolumeSee);
    }

    [Fact]
    public void FullVolumeDefaultsPreserveStockSoundsWhenOnlyOneNameIsOverridden()
    {
        var patch = DehackedPatch.Apply("Thing 34\nBits = FULLVOLSOUNDS\nDeath sound = test/death\n");
        var (sim, missile) = Room(patch, ProjectileKind.Rocket);
        Assert.Equal("test/death", missile.ActorSounds[ActorSoundType.Death]);
        Assert.True(missile.ActorSounds.ContainsKey(ActorSoundType.See));
        sim.DrainSoundRequests(); ActorSoundActions.SeeSound(missile);
        Assert.Equal(0, Assert.Single(sim.DrainSoundRequests()).Attenuation);
        ActorSoundActions.Scream(missile); var death = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("test/death", death.Sound); Assert.Equal(0, death.Attenuation);
    }

    [Fact]
    public void InheritedSettingPersistsUntilNextBitsAssignment()
    {
        var original = DehackedPatch.Apply("Thing 34\nBits = FULLVOLSOUNDS\n");
        var inherited = DehackedPatch.Apply("Thing 34\nMass = 200\n", original);
        var cleared = DehackedPatch.Apply("Thing 34\nBits = NOGRAVITY\n", inherited);
        Assert.True(Room(inherited, ProjectileKind.Rocket).Missile.FullVolumeDeath);
        Assert.False(Room(cleared, ProjectileKind.Rocket).Missile.FullVolumeDeath);
        Assert.True(original.Actors.Single(a => a.Index == 34).FullVolumeSounds);
    }

    [Fact]
    public void UnrelatedPatchRetainsConstructorVolumeDefaults()
    {
        var (sim, missile) = Room(DehackedPatch.Apply("Thing 34\nMass = 200\n"), ProjectileKind.Rocket);
        var stock = new ProjectileActor(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(stock.FullVolumeDeath, missile.FullVolumeDeath); Assert.Equal(stock.FullVolumeSee, missile.FullVolumeSee);
        Assert.Equal(stock.ActorSounds.OrderBy(p => p.Key), missile.ActorSounds.OrderBy(p => p.Key));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RuntimeVolumeOverridesSaveIndependentlyOfClassDefaults(bool fullVolume)
    {
        var patch = DehackedPatch.Apply($"Thing 34\nBits = {(fullVolume ? "FULLVOLSOUNDS+NOGRAVITY" : "NOGRAVITY")}\n");
        var (sim, missile) = Room(patch, ProjectileKind.Rocket);
        missile.FullVolumeDeath = missile.FullVolumeSee = !fullVolume;
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch, ProjectileKind.Rocket);
        SimSavegame.Apply(fresh, saved);
        Assert.Equal(!fullVolume, loaded.FullVolumeDeath); Assert.Equal(!fullVolume, loaded.FullVolumeSee);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); ActorRaise.ReviveSupported(loaded);
        Assert.Equal(fullVolume, loaded.FullVolumeDeath); Assert.Equal(fullVolume, loaded.FullVolumeSee);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(DehackedPatchResult patch, ProjectileKind kind)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), kind));
    }
}
