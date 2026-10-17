using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNumericSoundTests
{
    [Theory]
    [InlineData("1", "weapons/pistol")]
    [InlineData("+12 trailing", "weapons/sawfull")]
    [InlineData("36", "grunt/sight")]
    [InlineData("37", "grunt/sight")]
    [InlineData("113", "dog/pain")]
    [InlineData("114", "")]
    [InlineData("499", "")]
    [InlineData("500", "dehextra/sound000")]
    [InlineData("699", "dehextra/sound199")]
    [InlineData("700", "")]
    [InlineData("2147483647", "")]
    public void NumericSpawnSoundUsesNativeCatalogAndSurvivesFreshSave(string number, string expected)
    {
        var patch = DehackedPatch.Apply($"Thing 34\nAlert sound = {number}\n");
        var (sim, missile) = Room(patch);
        Assert.Equal(expected, missile.ActorSounds[ActorSoundType.See]);
        if (expected.Length == 0) Assert.Empty(sim.DrainSoundRequests());
        else Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()).Sound);
        var saved = SimSavegame.Write(sim, true);
        var (fresh, loaded) = Room(patch); fresh.DrainSoundRequests(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        ActorSoundActions.PlaySpawnSound(loaded);
        if (expected.Length == 0) Assert.Empty(fresh.DrainSoundRequests());
        else Assert.Equal(expected, Assert.Single(fresh.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData("Alert", ActorSoundType.See)]
    [InlineData("Attack", ActorSoundType.Attack)]
    [InlineData("Pain", ActorSoundType.Pain)]
    [InlineData("Death", ActorSoundType.Death)]
    [InlineData("Action", ActorSoundType.Active)]
    [InlineData("Rip", ActorSoundType.Rip)]
    public void AllSelectorsReplaceNamedDefaultsWithoutMutatingBaseline(string field, ActorSoundType slot)
    {
        var original = DehackedPatch.Apply($"Thing 34\n{field} sound = custom/original\n");
        var numeric = DehackedPatch.Apply($"Thing 34\n{field} sound = 1\n", original);
        var inherited = DehackedPatch.Apply("Thing 34\nMass = 200\n", numeric);
        var (_, missile) = Room(inherited);
        Assert.Equal("weapons/pistol", missile.ActorSounds[slot]);
        Assert.Equal("custom/original", original.Actors.Single(a => a.Index == 34).SoundNameOverrides[(int)slot]);
        var named = DehackedPatch.Apply($"Thing 34\n{field} sound = custom/replacement\n", inherited);
        Assert.Equal("custom/replacement", Room(named).Missile.ActorSounds[slot]);
    }

    [Fact]
    public void NumericRipSoundPlaysOnContactAfterRestore()
    {
        var patch = DehackedPatch.Apply("Thing 35\nBits = RIP+NOGRAVITY\nRip sound = 1\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved); sim.DrainSoundRequests();
        missile.DamageExpression = _ => 7; missile.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(993, target.Health);
        var sound = Assert.Single(sim.DrainSoundRequests()); Assert.Equal("weapons/pistol", sound.Sound);
        Assert.Equal(4, sound.Channel); Assert.Equal(1.001f, sound.Attenuation);
    }

    [Fact]
    public void NumericDeathSoundPlaysAndMapAndDynamicActorsReceiveCatalogDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 2\nDeath sound = 1\n\nThing 34\nDeath sound = 500\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        Assert.Equal("weapons/pistol", sim.Actors.Single().ActorSounds[ActorSoundType.Death]);
        var actor = sim.AddBot(200, 0);
        ActorSoundActions.Scream(actor);
        Assert.Equal("weapons/pistol", Assert.Single(sim.DrainSoundRequests()).Sound);
        var (projectileSim, missile) = Room(patch); projectileSim.DrainSoundRequests();
        ActorSoundActions.Scream(missile);
        Assert.Equal("dehextra/sound000", Assert.Single(projectileSim.DrainSoundRequests()).Sound);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket));
    }
}
