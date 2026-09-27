using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponDamageSpreadTests
{
    [Theory]
    [InlineData(WeaponKind.Pistol, 5, 15)]
    [InlineData(WeaponKind.Chaingun, 5, 15)]
    [InlineData(WeaponKind.Fist, 2, 20)]
    [InlineData(WeaponKind.Chainsaw, 2, 20)]
    public void NativeDamageDiceVaryAcrossSeeds(WeaponKind weapon, int unit, int maximum)
    {
        var rolls = new HashSet<int>();
        for (var seed = 0; seed < 64; seed++)
        {
            var (sim, player, victim) = Range(seed);
            player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
            Assert.True(HitscanCombat.Fire(sim, player));
            var damage = 1000 - victim.Health;
            Assert.InRange(damage, unit, maximum);
            Assert.Equal(0, damage % unit);
            rolls.Add(damage);
        }
        Assert.True(rolls.Count > 1);
    }

    [Fact]
    public void SuperShotgunSpreadsVerticallyWhileShotgunStaysLevel()
    {
        foreach (var weapon in new[] { WeaponKind.Shotgun, WeaponKind.SuperShotgun })
        {
            var (sim, player, victim) = Range(42);
            victim.X = Fixed.FromInt(500); victim.Radius = Fixed.FromInt(150);
            victim.Z = Fixed.FromInt(40); victim.Height = Fixed.FromInt(40);
            player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon; player.Inventory.Shells = 2;
            Assert.True(HitscanCombat.Fire(sim, player));
            if (weapon == WeaponKind.Shotgun) Assert.Equal(1000, victim.Health);
            else Assert.True(victim.Health < 1000);
        }
    }

    [Fact]
    public void ThinTargetDoesNotCatchEverySuperShotgunPellet()
    {
        var (sim, player, victim) = Range(42);
        victim.X = Fixed.FromInt(500); victim.Radius = Fixed.FromInt(150);
        victim.Z = Fixed.FromInt(27); victim.Height = Fixed.FromInt(2);
        player.Inventory.Weapons |= WeaponKind.SuperShotgun;
        player.Inventory.Selected = WeaponKind.SuperShotgun; player.Inventory.Shells = 2;
        HitscanCombat.Fire(sim, player);
        Assert.True(1000 - victim.Health < 100); // Twenty guaranteed hits would deal at least 100.
    }

    [Fact]
    public void DryFireDoesNotConsumeCombatRandomness()
    {
        var (sim, player, _) = Range(42);
        player.Inventory.Bullets = 0;
        var before = sim.CombatRandomState;
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.Equal(before, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(3000, true)]
    [InlineData(8212, true)]
    [InlineData(8213, false)]
    public void PlayerBulletRangeExtendsToNative8192UnitSurface(int x, bool hit)
    {
        var (sim, player, victim) = Range(42);
        victim.X = Fixed.FromInt(x);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(hit, victim.Health < 1000);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Fact]
    public void DamageAndVerticalSpreadReplayFromSameSeed()
    {
        var (left, a, victimA) = Range(53); var (right, b, victimB) = Range(53);
        foreach (var player in new[] { a, b })
        {
            player.Inventory.Weapons |= WeaponKind.SuperShotgun;
            player.Inventory.Selected = WeaponKind.SuperShotgun; player.Inventory.Shells = 2;
        }
        HitscanCombat.Fire(left, a); HitscanCombat.Fire(right, b);
        Assert.Equal(victimA.Health, victimB.Health);
        Assert.Equal(left.CombatRandomState, right.CombatRandomState);
        left.Tick(); right.Tick(); Assert.Equal(left.Checksum, right.Checksum);
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) Range(int seed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: seed);
        var victim = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        victim.Brain = null; victim.Health = 1000; victim.PainChance = 0; victim.NoGravity = true;
        return (sim, sim.Players.Single(), victim);
    }
}
