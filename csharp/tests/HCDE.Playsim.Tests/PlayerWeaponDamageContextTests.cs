using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerWeaponDamageContextTests
{
    [Theory]
    [InlineData(WeaponKind.Fist)]
    [InlineData(WeaponKind.Chainsaw)]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Chaingun)]
    [InlineData(WeaponKind.Shotgun)]
    [InlineData(WeaponKind.SuperShotgun)]
    public void FiringUsesNativeTypedDamageAfterSave(WeaponKind weapon)
    {
        var melee = weapon is WeaponKind.Fist or WeaponKind.Chainsaw;
        foreach (var factor in new[] { 0d, 2d })
        {
            var (baseline, first) = Room(weapon); var (sim, target) = Room(weapon);
            target.SetDamageFactor(melee ? "Melee" : "Hitscan", factor);
            target.SetDamageFactor(melee ? "Hitscan" : "Melee", 0);
            sim.Players.Single().DamageType = melee ? "Hitscan" : "Melee";
            SimSavegame.Apply(sim, SimSavegame.Write(sim));
            Assert.True(HitscanCombat.Fire(baseline, baseline.Players.Single()));
            Assert.True(HitscanCombat.Fire(sim, sim.Players.Single()));
            var ordinaryDamage = 1000 - first.Health;
            Assert.True(ordinaryDamage > 0);
            Assert.Equal(1000 - (int)(ordinaryDamage * factor), target.Health);
            Assert.Equal(baseline.CombatRandomState, sim.CombatRandomState);
            var expected = baseline.Players.Single(); var actual = sim.Players.Single();
            Assert.Equal(expected.WeaponCooldown, actual.WeaponCooldown);
            Assert.Equal(expected.Inventory.Bullets, actual.Inventory.Bullets);
            Assert.Equal(expected.Inventory.Shells, actual.Inventory.Shells);
        }
    }

    private static (AuthoritySimulation Sim, Actor Target) Room(WeaponKind weapon)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon;
        player.Inventory.Selected = weapon; player.Inventory.TryAddAmmo(AmmoKind.Shells, 50);
        var melee = weapon is WeaponKind.Fist or WeaponKind.Chainsaw;
        var target = sim.AddBot(melee ? 48 : 250, 0, 3003); target.Brain = null;
        target.Health = 1000; target.NoPain = true;
        if (!melee) { target.Radius = Fixed.FromInt(100); target.Height = Fixed.FromInt(200); }
        return (sim, target);
    }
}
