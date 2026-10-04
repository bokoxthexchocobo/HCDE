using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropAmmoFactorTests
{
    [Theory]
    [InlineData(-1, PickupCatalog.Shotgun, 4, false, 8)]
    [InlineData(0.75, PickupCatalog.Shotgun, 6, true, 6)]
    [InlineData(0, PickupCatalog.Shotgun, 0, true, 0)]
    [InlineData(0.75, PickupCatalog.Clip, 7, true, 57)]
    [InlineData(0, PickupCatalog.Clip, 1, true, 51)]
    public void DropFactorControlsAmountAndSkillBypass(double factor, int type,
        int amount, bool ignoreSkill, int received)
    {
        var sim = Room(); sim.DropAmmoFactor = factor;
        var player = sim.Players.Single();
        Assert.True(sim.SpawnDroppedPickup(player, type));
        var drop = sim.Actors[^1];
        Assert.Equal(amount, drop.PickupAmount);
        Assert.Equal(ignoreSkill, drop.IgnoreAmmoSkill);
        Assert.True(PickupCatalog.TryGive(player, type, drop.IgnoreAmmoSkill,
            pickupAmount: drop.PickupAmount, suppressWeaponAmmo: drop.SuppressWeaponPickupAmmo));
        Assert.Equal(received, type == PickupCatalog.Clip ? player.Inventory.Bullets : player.Inventory.Shells);
    }

    [Fact]
    public void ExplicitAmmoAmountScalingToZeroDoesNotFallBackToDefault()
    {
        var sim = Room(); sim.DropAmmoFactor = 0;
        var player = sim.Players.Single();
        Assert.True(sim.SpawnDroppedPickup(player, PickupCatalog.Clip, pickupAmount: 3));
        var drop = sim.Actors[^1];
        Assert.Equal(0, drop.PickupAmount);
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Clip, drop.IgnoreAmmoSkill,
            pickupAmount: drop.PickupAmount, suppressWeaponAmmo: drop.SuppressWeaponPickupAmmo));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        sim.RestoreState(state);
        Assert.True(sim.Actors[^1].SuppressWeaponPickupAmmo);
    }

    [Fact]
    public void ExplicitDeathDropAmountBypassesCustomFactor()
    {
        var sim = Room(); sim.DropAmmoFactor = 0;
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Shotgun,
            ignoreAmmoSkill: true, pickupAmount: 17));
        Assert.Equal(17, sim.Actors[^1].PickupAmount);
        Assert.False(sim.Actors[^1].SuppressWeaponPickupAmmo);
    }

    [Fact]
    public void FactorChangesChecksumAndSurvivesRestore()
    {
        var normal = Room(); var configured = Room(); configured.DropAmmoFactor = 0.75;
        normal.Tick(); configured.Tick();
        Assert.NotEqual(normal.Checksum, configured.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(configured), out var state, out var error), error);
        configured.RestoreState(state);
        Assert.Equal(0.75, configured.DropAmmoFactor);
        Assert.Throws<ArgumentOutOfRangeException>(() => configured.DropAmmoFactor = double.NaN);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Skill: 0));
}
