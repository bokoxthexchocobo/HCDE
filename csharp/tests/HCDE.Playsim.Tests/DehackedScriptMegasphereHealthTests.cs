using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedScriptMegasphereHealthTests
{
    [Theory]
    [InlineData(300, 0, 300)]
    [InlineData(150, 0, 150)]
    [InlineData(300, 20, 320)]
    public void ScriptHealthUsesPatchedCapAndBonus(int cap, int bonus, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply($"Misc 0\nMegasphere Health = {cap}\n"));
        var player = sim.Players.Single(); player.BonusHealth = bonus;
        AcsPlayerInventory.Give(player, "megaspherehealth", 500);
        Assert.Equal(expected, player.Health); Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void ScriptGrantRemainsAdditiveAndDoesNotReduceOverCapHealth()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Misc 0\nMegasphere Health = 150\n"));
        var player = sim.Players.Single(); AcsPlayerInventory.Give(player, "MegasphereHealth", 10);
        Assert.Equal(110, player.Health);
        player.Health = 200; AcsPlayerInventory.Give(player, "MegasphereHealth", 10);
        Assert.Equal(200, player.Health);
    }
}
