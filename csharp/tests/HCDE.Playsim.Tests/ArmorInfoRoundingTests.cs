using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class ArmorInfoRoundingTests
{
    [Theory]
    [InlineData(1, 655)]
    [InlineData(2, 1311)]
    [InlineData(33, 21846)]
    [InlineData(50, 32768)]
    [InlineData(99, 64881)]
    [InlineData(100, 65536)]
    public void ArmorPercentUsesNativeFixedConversionThroughAcs(int percent, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = percent;
        Assert.Equal(expected, Query(sim));
    }

    [Fact]
    public void DepletedArmorDoesNotReportRetainedPercent()
    {
        var sim = Room(); sim.Players.Single().Inventory.ArmorSavePercent = 99;
        Assert.Equal(0, Query(sim));
    }

    [Fact]
    public void SavedArmorRetainsRoundedQueryResult()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory; inventory.Armor = 100; inventory.ArmorSavePercent = 99;
        var saved = SimSavegame.Write(sim, true); inventory.ArmorSavePercent = 50; SimSavegame.Apply(sim, saved);
        Assert.Equal(64881, Query(sim));
    }

    private static int Query(AuthoritySimulation sim)
    {
        var stack = new List<int> { 77, 2 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = sim.Players.Single() }, [], new(),
            AcsCallFunctions.GetArmorInfo, 1, out var result)); Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
