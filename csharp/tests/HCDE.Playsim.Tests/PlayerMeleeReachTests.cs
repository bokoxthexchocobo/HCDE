using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerMeleeReachTests
{
    [Theory]
    [InlineData(WeaponKind.Fist, 4, 48, false)]
    [InlineData(WeaponKind.Chainsaw, 4, 48, false)]
    [InlineData(WeaponKind.Fist, 100, 100, true)]
    [InlineData(WeaponKind.Chainsaw, 100, 100, true)]
    [InlineData(WeaponKind.Fist, 10.5, 40, true)]
    [InlineData(WeaponKind.Chainsaw, 10.5, 40, true)]
    [InlineData(WeaponKind.Pistol, 0, 250, true)]
    public void FiringHonorsConfiguredMeleeReachWithoutChangingBulletRange(WeaponKind weapon, double reach, int x, bool hit)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon;
        player.Inventory.Selected = weapon;
        AcsActorProperties.Set(sim, player, 0, AcsActorProperties.MeleeRange, Fixed.FromDouble(reach).Raw);
        Assert.Equal(Fixed.FromDouble(reach).Raw, AcsActorProperties.Get(sim, player, 0, AcsActorProperties.MeleeRange));
        var target = sim.AddBot(x, 0, 3001); target.Brain = null;
        target.Health = 1000; target.NoPain = true; target.Radius = Fixed.FromInt(16);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(hit, target.Health < 1000);
        Assert.Equal(weapon == WeaponKind.Pistol ? 49 : 50, player.Inventory.Bullets);
        Assert.Equal(WeaponCatalog.Find(weapon)!.RefireTics, player.WeaponCooldown);
    }
}
