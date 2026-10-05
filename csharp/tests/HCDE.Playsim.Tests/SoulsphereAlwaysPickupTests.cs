using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoulsphereAlwaysPickupTests
{
    [Theory]
    [InlineData(200, 200)]
    [InlineData(200, 250)]
    [InlineData(300, 300)]
    [InlineData(150, 200)]
    public void ContactConsumesSphereWithoutReducingHealthAtOrAboveCap(int cap, int health)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Soulsphere }],
        }, dehacked: DehackedPatch.Apply($"Misc 0\nMax Soulsphere = {cap}\n"));
        var player = sim.Players.Single(); var item = sim.Actors[^1]; player.Health = health;
        sim.Tick(); Assert.True(item.Destroyed); Assert.DoesNotContain(item, sim.Actors);
        Assert.Equal(health, player.Health);
    }
}
