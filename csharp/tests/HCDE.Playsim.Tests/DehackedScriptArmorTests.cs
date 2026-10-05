using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedScriptArmorTests
{
    [Theory]
    [InlineData("greenarmor", "Green Armor Class", 2, 1, 200, 50)]
    [InlineData("GreenArmor", "Green Armor Class", 3, 2, 600, 50)]
    [InlineData("bluearmor", "Blue Armor Class", 1, 2, 200, 33)]
    public void ScriptSuitAmountUsesPatchedClass(string name, string setting, int armorClass, int count, int expected, int percent)
    {
        var sim = Room($"Misc 0\n{setting} = {armorClass}\n");
        var player = sim.Players.Single(); AcsPlayerInventory.Give(player, name, count);
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(percent, player.Inventory.ArmorSavePercent);
        Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void ScriptMegasphereArmorRemainsIndependentOfBlueClass()
    {
        var player = Room("Misc 0\nBlue Armor Class = 1\n").Players.Single();
        AcsPlayerInventory.Give(player, "BlueArmorForMegasphere", 2);
        Assert.Equal(400, player.Inventory.Armor); Assert.Equal(50, player.Inventory.ArmorSavePercent);
    }

    private static AuthoritySimulation Room(string patch) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: DehackedPatch.Apply(patch));
}
