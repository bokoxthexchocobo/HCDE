using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryClearDispatchTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingActivatorOrTidZeroClearsEveryPlayer(bool actorOpcode)
    {
        var sim = Room();
        foreach (var player in sim.Players) player.Inventory.BlueKey = true;
        Run(sim, actorOpcode ? sim.Players.First() : null, actorOpcode
            ? [(int)AcsPcode.PushNumber, 0, (int)AcsPcode.ClearActorInventory]
            : [(int)AcsPcode.ClearInventory]);
        Assert.All(sim.Players, p => Assert.False(p.Inventory.BlueKey));
    }

    [Theory]
    [InlineData(9, false)]
    [InlineData(999, true)]
    public void NonzeroTidOnlyClearsMatchingPlayer(int tid, bool targetKeepsKey)
    {
        var sim = Room();
        foreach (var player in sim.Players) player.Inventory.BlueKey = true;
        Run(sim, null, (int)AcsPcode.PushNumber, tid, (int)AcsPcode.ClearActorInventory);
        Assert.True(sim.Players.First().Inventory.BlueKey);
        Assert.Equal(targetKeepsKey, sim.Players.Last().Inventory.BlueKey);
    }

    [Fact]
    public void PresentActivatorDoesNotBroadcastClear()
    {
        var sim = Room();
        foreach (var player in sim.Players) player.Inventory.BlueKey = true;
        Run(sim, sim.Players.First(), (int)AcsPcode.ClearInventory);
        Assert.False(sim.Players.First().Inventory.BlueKey);
        Assert.True(sim.Players.Last().Inventory.BlueKey);
    }

    private static void Run(AuthoritySimulation sim, Actor? activator, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, Id = 9, X = 400 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
