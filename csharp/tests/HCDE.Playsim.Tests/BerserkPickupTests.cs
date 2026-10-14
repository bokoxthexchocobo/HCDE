using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BerserkPickupTests
{
    [Theory]
    [InlineData(10, 100)]
    [InlineData(100, 100)]
    [InlineData(150, 150)]
    public void PickupGrantsStrengthHealsAndRequestsFist(int health, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = health;
        var item = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.Berserk);
        sim.Tick();
        Assert.True(item.Destroyed); Assert.Equal(expected, player.Health); Assert.True(player.HasPowerStrength);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void RepeatedGrantResetsTimerAtFullHealth()
    {
        var sim = Room(false); var player = sim.Players.Single(); player.GivePowerStrength(); player.Tick();
        Assert.True(ActorInventoryActions.GiveInventory(player, "Berserk"));
        Assert.Equal(1, player.PowerStrengthTics); Assert.Equal(100, player.Health);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }

    [Fact]
    public void AcsGrantUsesSameEffectsAndClassMapping()
    {
        var sim = Room(false); var player = sim.Players.Single(); player.Health = 30;
        AcsPlayerInventory.Give(player, "berserk", 1);
        Assert.True(player.HasPowerStrength); Assert.Equal(100, player.Health);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
        Assert.True(PickupCatalog.TryEditorNumberForDropName("Berserk", out var number));
        Assert.Equal(PickupCatalog.Berserk, number);
    }

    [Fact]
    public void PickupAndWeaponSelectionReplayAfterRestore()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 25;
        var saved = SimSavegame.Write(sim, true); sim.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(expected, SimSavegame.Write(sim, true));
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(WeaponKind.Fist, player.Inventory.Selected); Assert.True(player.WeaponReady);
    }

    [Fact]
    public void DeadPlayerCannotReceiveBerserkEffects()
    {
        var sim = Room(false); var player = sim.Players.Single(); player.Health = 0;
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.Berserk)); Assert.False(player.HasPowerStrength);
    }

    private static AuthoritySimulation Room(bool pickup = true) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, .. (pickup
            ? new[] { new LevelThing { Type = PickupCatalog.Berserk } } : Array.Empty<LevelThing>())]
    });
}
