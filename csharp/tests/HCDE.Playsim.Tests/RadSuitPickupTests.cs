using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class RadSuitPickupTests
{
    [Fact]
    public void MapPickupHasNativeGeometryBrightnessAndGrantsSuit()
    {
        var sim = Room(); var item = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.RadSuit);
        Assert.Equal(46, item.Height.ToDouble()); Assert.True(item.FullBright); Assert.True(item.SpecialPickup);
        sim.Tick(); Assert.True(item.Destroyed); Assert.Equal(2100, sim.Players.Single().PowerIronFeetTics);
        Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "misc/p_pkup");
    }

    [Theory]
    [InlineData(1000, 2100)]
    [InlineData(2500, 2500)]
    [InlineData(128, 2100)]
    public void AutoactivatedGiverAlwaysRefreshesWithoutShorteningLongerPower(int current, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.PowerIronFeetTics = current + 1;
        var item = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.RadSuit); sim.Tick();
        Assert.True(item.Destroyed); Assert.Equal(expected, player.PowerIronFeetTics);
    }

    [Fact]
    public void ScriptGrantAndClassMappingUseSuitPower()
    {
        var sim = Room(false); var player = sim.Players.Single(); AcsPlayerInventory.Give(player, "RadSuit", 1);
        Assert.Equal(2100, player.PowerIronFeetTics); player.PowerIronFeetTics = 0;
        Assert.True(ActorInventoryActions.GiveInventory(player, "RadSuit")); Assert.Equal(2100, player.PowerIronFeetTics);
        Assert.True(PickupCatalog.TryEditorNumberForDropName("RadSuit", out var number)); Assert.Equal(2025, number);
    }

    [Fact]
    public void PickupReplayAfterSaveIsExact()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); sim.Tick(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void DeadPlayerCannotReceiveSuitPickup()
    {
        var sim = Room(false); var player = sim.Players.Single(); player.Health = 0;
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.RadSuit)); Assert.Equal(0, player.PowerIronFeetTics);
    }

    private static AuthoritySimulation Room(bool pickup = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = pickup ? [new LevelThing { Type = 1 }, new LevelThing { Type = 2025 }] : [new LevelThing { Type = 1 }]
    });
}
