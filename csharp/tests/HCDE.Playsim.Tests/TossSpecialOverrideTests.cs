using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TossSpecialOverrideTests
{
    [Theory]
    [InlineData("Clip", PickupCatalog.Clip)]
    [InlineData("Backpack", PickupCatalog.Backpack)]
    [InlineData("Shotgun", PickupCatalog.Shotgun)]
    public void ScriptCanEnableCollectionBeforeDelayExpires(string type, int editorNumber)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, type, 1);
        Assert.True(AcsPlayerInventory.Drop(player, type));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == editorNumber);
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        Assert.True(AcsActorFlags.TrySet(drop, "SPECIAL", true));
        Assert.Equal(30, drop.PickupDelay);
        sim.Tick();
        Assert.DoesNotContain(drop, sim.Actors);
        Assert.Equal(29, drop.PickupDelay);
        if (type == "Backpack") Assert.True(player.Inventory.HasBackpack);
        if (type == "Shotgun") Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
    }

    [Fact]
    public void ExpiryRestoresCatalogSpecialFlagAfterScriptClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Drop(player, "Clip");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        drop.X = Fixed.FromInt(500); drop.VelocityX = default;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.True(AcsActorFlags.TrySet(drop, "SPECIAL", false));
        sim.Tick();
        Assert.True(drop.SpecialPickup);
        Assert.Equal(0, drop.PickupDelay);
    }

    [Fact]
    public void SavedScriptOverrideRemainsEffectiveWithPositiveDelay()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Drop(player, "Clip");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        AcsActorFlags.TrySet(drop, "SPECIAL", true);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        drop.SpecialPickup = false;
        sim.RestoreState(state);
        Assert.True(drop.SpecialPickup);
        Assert.Equal(30, drop.PickupDelay);
        sim.Tick();
        Assert.DoesNotContain(drop, sim.Actors);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
