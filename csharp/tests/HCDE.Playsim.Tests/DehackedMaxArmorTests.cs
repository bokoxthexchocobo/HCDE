using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMaxArmorTests
{
    private static AuthoritySimulation Room(int maximum = 200) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: DehackedPatch.Apply($"Misc 0\nMax Armor = {maximum}\n"));

    [Theory]
    [InlineData(300, 299, 300, true)]
    [InlineData(120, 119, 120, true)]
    [InlineData(120, 200, 200, false)]
    [InlineData(0, 0, 0, true)]
    [InlineData(-10, 0, 0, true)]
    public void BonusUsesPatchedCapWithoutReducingExistingArmor(int maximum, int initial, int expected, bool success)
    {
        var player = Room(maximum).Players.Single(); player.Inventory.Armor = initial;
        Assert.Equal(success, PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus, pickupAmount: 10));
        Assert.Equal(expected, player.Inventory.Armor);
    }

    [Fact]
    public void FreshBonusStoresPatchedArmorLimits()
    {
        var player = Room(300).Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus));
        Assert.Equal(300, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal(300, player.Inventory.ArmorMaximum);
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void BaselinePreservesCapAndInvalidValueReportsError()
    {
        var patch = DehackedPatch.Apply("Misc 0\nMax Armor = 300\n");
        Assert.Equal(300, DehackedPatch.Apply("", patch).MaxArmor);
        var invalid = DehackedPatch.Apply("Misc 0\nMax Armor = invalid\n", patch);
        Assert.NotEmpty(invalid.Errors); Assert.Equal(300, invalid.MaxArmor);
    }

    [Fact]
    public void CapChangesChecksum()
    {
        var baseline = Room(); var patched = Room(300);
        baseline.Tick(); patched.Tick(); Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }
}
