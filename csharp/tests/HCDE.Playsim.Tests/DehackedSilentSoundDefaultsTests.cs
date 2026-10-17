using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSilentSoundDefaultsTests
{
    [Theory]
    [InlineData("Alert", 0)]
    [InlineData("Attack", 1)]
    [InlineData("Pain", 2)]
    [InlineData("Death", 3)]
    [InlineData("Action", 4)]
    public void NumericNoSoundRemainsExplicitThroughPatchLayering(string field, int selector)
    {
        var original = DehackedPatch.Apply($"Thing 34\n{field} sound = custom/test\n");
        var silent = DehackedPatch.Apply($"Thing 34\n{field} sound = 0\n", original); Assert.Empty(silent.Errors);
        Assert.Equal("", silent.Actors.Single(a => a.Index == 34).SoundNameOverrides[selector]);
        var inherited = DehackedPatch.Apply("Thing 34\nMass = 200\n", silent);
        Assert.Equal("", inherited.Actors.Single(a => a.Index == 34).SoundNameOverrides[selector]);
        Assert.Equal("custom/test", original.Actors.Single(a => a.Index == 34).SoundNameOverrides[selector]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("+0")]
    [InlineData("-1")]
    [InlineData("0 trailing")]
    public void SilentProjectileDefaultsSuppressSpawnAndDeathAndSurviveFreshSave(string number)
    {
        var patch = DehackedPatch.Apply($"Thing 34\nAlert sound = {number}\nDeath sound = {number}\n");
        var (sim, missile) = Room(patch); Assert.Empty(sim.PendingSoundRequests);
        Assert.Equal("", missile.ActorSounds[ActorSoundType.See]); Assert.Equal("", missile.ActorSounds[ActorSoundType.Death]);
        ActorSoundActions.SeeSound(missile); ActorSoundActions.Scream(missile); Assert.Empty(sim.PendingSoundRequests);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        ActorSoundActions.PlaySpawnSound(loaded); ActorSoundActions.Scream(loaded); Assert.Empty(fresh.PendingSoundRequests);
    }

    [Fact]
    public void SilencingOnlyDeathPreservesStockSpawnSound()
    {
        var (sim, missile) = Room(DehackedPatch.Apply("Thing 34\nDeath sound = 0\n"));
        Assert.Single(sim.DrainSoundRequests()); Assert.NotEmpty(missile.ActorSounds[ActorSoundType.See]);
        ActorSoundActions.Scream(missile); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void LaterNamedSoundReplacesExplicitSilence()
    {
        var silent = DehackedPatch.Apply("Thing 34\nDeath sound = 0\n");
        var named = DehackedPatch.Apply("Thing 34\nDeath sound = custom/death\n", silent);
        var (sim, missile) = Room(named); sim.DrainSoundRequests(); ActorSoundActions.Scream(missile);
        Assert.Equal("custom/death", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket));
    }
}
