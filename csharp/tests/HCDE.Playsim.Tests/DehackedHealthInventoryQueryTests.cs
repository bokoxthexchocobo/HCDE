using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedHealthInventoryQueryTests
{
    [Theory]
    [InlineData("healthbonus", -1)]
    [InlineData("soulsphere", 250)]
    [InlineData("megaspherehealth", 300)]
    public void MaximumQueryUsesPatchedClassDefault(string name, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Misc 0\nMax Soulsphere = 250\nMegasphere Health = 300\n"));
        var player = sim.Players.Single(); player.BonusHealth = 20; player.MaxPickupHealth = 400;
        Assert.Equal(expected, AcsPlayerInventory.Count(player, name, true));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        var monster = sim.AddBot(64, 0);
        Assert.Equal(expected, AcsPlayerInventory.Count(monster, [name], 0, true));
    }

    [Fact]
    public void UnpatchedBonusRetainsVanillaMaximum()
    {
        Assert.Equal(200, AcsPlayerInventory.Count(new PlayerPawn(), "HealthBonus", true));
    }
}
