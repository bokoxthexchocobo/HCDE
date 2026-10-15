using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsPooledMaxInventoryTests
{
    [Theory]
    [InlineData("Clip", 200)]
    [InlineData("Shell", 50)]
    [InlineData("PowerIronFeet", 1)]
    [InlineData("PowerMask", 1)]
    [InlineData("Health", 100)]
    [InlineData("MissingInventoryClass", 0)]
    public void PooledNamesMatchModuleCapacity(string name, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); var strings = new AcsGlobalStrings();
        foreach (var pooled in new[] { false, true })
        {
            var id = pooled ? strings.Add(name.ToLowerInvariant()) : 0;
            Assert.Equal(expected, Query(sim, player, 0, id, [name], strings));
        }
    }

    [Fact]
    public void TidTargetUsesItsBackpackCapacity()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ThingId = 7;
        player.Inventory.GiveBackpack(depleted: true); var strings = new AcsGlobalStrings();
        Assert.Equal(400, Query(sim, null, 7, strings.Add("Clip"), [], strings));
        Assert.Equal(0, Query(sim, player, 999, strings.Add("Clip"), [], strings));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(AcsStringIds.StringPoolMarker + 99)]
    public void InvalidNamesHaveZeroCapacity(int id)
    {
        var sim = Room(); Assert.Equal(0, Query(sim, sim.Players.Single(), 0, id, [], new()));
    }

    private static int Query(AuthoritySimulation sim, Actor? actor, int tid, int id, string[] module, AcsGlobalStrings strings)
    {
        var stack = new List<int> { 77, tid, id };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, module, strings,
            AcsCallFunctions.GetMaxInventory, 2, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
