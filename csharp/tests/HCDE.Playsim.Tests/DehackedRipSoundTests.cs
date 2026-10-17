using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedRipSoundTests
{
    [Theory]
    [InlineData("custom/rip", 7, true)]
    [InlineData("custom/rip", 0, false)]
    [InlineData("custom/rip", -7, false)]
    [InlineData("0", 7, false)]
    [InlineData("None", 7, false)]
    public void PatchedRipSoundPlaysOnRepresentedRippingContactAfterSave(string sound, int damage, bool plays)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nBits = RIP+NOGRAVITY\nRip sound = {sound}\n");
        var (sim, missile, target) = Room(patch);
        var saved = SimSavegame.Write(sim, true); missile.ActorSounds = new Dictionary<ActorSoundType, string>();
        SimSavegame.Apply(sim, saved); sim.DrainSoundRequests(); missile.DamageExpression = _ => damage;
        missile.Tick(); var sounds = sim.DrainSoundRequests(); Assert.False(missile.Destroyed);
        if (plays)
        {
            var request = Assert.Single(sounds); Assert.Equal("custom/rip", request.Sound);
            Assert.Equal(4, request.Channel); Assert.Equal(1.001f, request.Attenuation); Assert.Equal(993, target.Health);
        }
        else Assert.Empty(sounds);
    }

    [Fact]
    public void NamedRipSoundSurvivesFreshProjectileReconstruction()
    {
        var patch = DehackedPatch.Apply("Thing 35\nRip sound = custom/rip\n");
        var (sim, missile, _) = Room(patch); var saved = SimSavegame.Write(sim, true);
        var (fresh, loaded, _) = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal("custom/rip", loaded.ActorSounds[ActorSoundType.Rip]); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(missile.ActorSounds.OrderBy(p => p.Key), loaded.ActorSounds.OrderBy(p => p.Key));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void NumericSilenceReplacesInheritedRipName(string number)
    {
        var original = DehackedPatch.Apply("Thing 35\nRip sound = custom/rip\n");
        var silent = DehackedPatch.Apply($"Thing 35\nRip sound = {number}\n", original); Assert.Empty(silent.Errors);
        Assert.Equal("", silent.Actors.Single(a => a.Index == 35).SoundNameOverrides[(int)ActorSoundType.Rip]);
        Assert.Equal("custom/rip", original.Actors.Single(a => a.Index == 35).SoundNameOverrides[(int)ActorSoundType.Rip]);
        Assert.Equal("", Room(silent).Missile.ActorSounds[ActorSoundType.Rip]);
    }

    [Fact]
    public void ParsedRipSoundBindsMapAndDynamicActorDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 2\nRip sound = custom/rip\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        Assert.Equal("custom/rip", sim.Actors.Single().ActorSounds[ActorSoundType.Rip]);
        Assert.Equal("custom/rip", sim.AddBot(200, 0).ActorSounds[ActorSoundType.Rip]);
    }

    [Fact]
    public void PositiveNumericSelectionReplacesInheritedNameAndRetainsIdentifier()
    {
        var original = DehackedPatch.Apply("Thing 35\nRip sound = custom/rip\n");
        var numeric = DehackedPatch.Apply("Thing 35\nRip sound = 12\n", original); Assert.Empty(numeric.Errors);
        var actor = numeric.Actors.Single(a => a.Index == 35); Assert.Equal(12, actor.RipSound);
        Assert.Equal("weapons/sawfull", actor.SoundNameOverrides[(int)ActorSoundType.Rip]);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        sim.DrainSoundRequests(); return (sim, missile, target);
    }
}
