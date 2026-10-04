using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathWeaponDropRandomTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(-1)]
    public void GuaranteedDeathDropConsumesChanceDrawBeforeToss(int seed)
    {
        var sim = Room(seed); var reference = Room(seed);
        reference.NextCombatRandom();
        reference.SpawnDroppedPickup(reference.Players.Single(), PickupCatalog.Pistol,
            ignoreAmmoSkill: true, pickupAmount: 50);
        sim.DropSelectedWeapon(sim.Players.Single());
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        var expected = Assert.Single(reference.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.Equal(expected.VelocityX, drop.VelocityX);
        Assert.Equal(expected.VelocityY, drop.VelocityY);
        Assert.Equal(expected.VelocityZ, drop.VelocityZ);
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void NoTossStillConsumesChanceDraw()
    {
        var sim = Room(0, CompatSurface.NoTossDrops);
        var reference = Room(0, CompatSurface.NoTossDrops);
        reference.NextCombatRandom();
        sim.DropSelectedWeapon(sim.Players.Single());
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.Equal(default, drop.VelocityX);
        Assert.Equal(default, drop.VelocityZ);
    }

    [Fact]
    public void StrifeStyleConsumesChanceThenFourMotionDraws()
    {
        var sim = Room(0); var reference = Room(0);
        sim.DropStyle = 2; reference.DropStyle = 2;
        reference.NextCombatRandom();
        reference.SpawnDroppedPickup(reference.Players.Single(), PickupCatalog.Pistol,
            ignoreAmmoSkill: true, pickupAmount: 50);
        sim.DropSelectedWeapon(sim.Players.Single());
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.Equal(24, drop.Z.ToDouble());
        Assert.Equal(default, drop.VelocityZ);
    }

    [Theory]
    [InlineData(false, WeaponKind.Pistol)]
    [InlineData(true, WeaponKind.Fist)]
    public void DisabledOrUnrepresentableDropDoesNotConsumeRandomness(bool enabled, WeaponKind selected)
    {
        var sim = Room(0); sim.WeaponDrop = enabled;
        var player = sim.Players.Single(); player.Inventory.Selected = selected;
        var random = sim.CombatRandomState;
        sim.DropSelectedWeapon(player);
        Assert.Equal(random, sim.CombatRandomState);
        Assert.Single(sim.Actors);
    }

    private static AuthoritySimulation Room(int seed, CompatSurface compat = CompatSurface.None)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, rngSeed: seed, compat: compat);
        sim.WeaponDrop = true;
        return sim;
    }
}
