using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryDropResultTests
{
    [Fact]
    public void SuccessfulActivatorDropReturnsZeroAndConsumesOnlyArguments()
    {
        var sim = Room(); var player = sim.Players.First();
        var stack = new List<int> { 123, 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = player },
            ["Clip"], new AcsGlobalStrings(), AcsCallFunctions.DropInventory, 2, out var result));
        Assert.Equal(0, result);
        Assert.Equal(new[] { 123 }, stack);
        Assert.Equal(49, player.Inventory.Bullets);
        Assert.Equal(50, sim.Players.Last().Inventory.Bullets);
    }

    [Fact]
    public void NonzeroTidDropsFromEveryMatchButStillReturnsZero()
    {
        var sim = Room();
        foreach (var player in sim.Players) player.ThingId = 7;
        Assert.Equal(0, Drop(sim, null, 7, 0));
        Assert.All(sim.Players, player => Assert.Equal(49, player.Inventory.Bullets));
    }

    [Fact]
    public void ZeroTidWithoutActivatorDoesNotBroadcast()
    {
        var sim = Room();
        Assert.Equal(0, Drop(sim, null, 0, 0));
        Assert.All(sim.Players, player => Assert.Equal(50, player.Inventory.Bullets));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void InvalidClassIndexReturnsZeroWithoutRemovingInventory(int index)
    {
        var sim = Room(); var player = sim.Players.First();
        Assert.Equal(0, Drop(sim, player, 0, index));
        Assert.Equal(50, player.Inventory.Bullets);
    }

    private static int Drop(AuthoritySimulation sim, Actor? actor, int tid, int index)
    {
        var stack = new List<int> { tid, index };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = actor },
            ["Clip"], new AcsGlobalStrings(), AcsCallFunctions.DropInventory, 2, out var result));
        Assert.Empty(stack);
        return result;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
