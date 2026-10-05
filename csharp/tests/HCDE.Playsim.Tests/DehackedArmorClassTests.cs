using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedArmorClassTests
{
    private static AuthoritySimulation Room(string patch) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: DehackedPatch.Apply(patch));

    [Theory]
    [InlineData(PickupCatalog.GreenArmor, "Green Armor Class", 2, 200, 50)]
    [InlineData(PickupCatalog.GreenArmor, "Green Armor Class", 3, 300, 50)]
    [InlineData(PickupCatalog.MegaArmor, "Blue Armor Class", 1, 100, 33)]
    [InlineData(PickupCatalog.MegaArmor, "Blue Armor Class", 3, 300, 50)]
    public void ClassControlsAmountAndAbsorption(int pickup, string setting, int value, int amount, int percent)
    {
        var player = Room($"Misc 0\n{setting} = {value}\n").Players.Single();
        Assert.True(PickupCatalog.TryGive(player, pickup));
        Assert.Equal(amount, player.Inventory.Armor); Assert.Equal(percent, player.Inventory.ArmorSavePercent);
        Assert.Equal(amount, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void BlueClassDoesNotChangeMegasphereArmor()
    {
        var player = Room("Misc 0\nBlue Armor Class = 1\n").Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal(50, player.Inventory.ArmorSavePercent);
    }

    [Theory]
    [InlineData("Green Armor Class")]
    [InlineData("Blue Armor Class")]
    public void InvalidIntegerReportsError(string setting) =>
        Assert.NotEmpty(DehackedPatch.Apply($"Misc 0\n{setting} = invalid\n").Errors);

    [Fact]
    public void BaselinePreservesBothClasses()
    {
        var patch = DehackedPatch.Apply("", DehackedPatch.Apply("Misc 0\nGreen Armor Class = 3\nBlue Armor Class = 1\n"));
        Assert.Equal(3, patch.GreenArmorClass); Assert.Equal(1, patch.BlueArmorClass);
    }

    [Theory]
    [InlineData("Green Armor Class")]
    [InlineData("Blue Armor Class")]
    public void EachClassChangesChecksum(string setting)
    {
        var baseline = Room("Misc 0\n"); var patched = Room($"Misc 0\n{setting} = 3\n");
        baseline.Tick(); patched.Tick(); Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }
}
