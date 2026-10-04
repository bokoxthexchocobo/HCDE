using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryBroadcastDispatchTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void MissingActivatorBroadcastsStackAndDirectInventoryOperations(bool take, bool direct)
    {
        var sim = Room();
        var words = direct
            ? new[] { (int)(take ? AcsPcode.TakeInventoryDirect : AcsPcode.GiveInventoryDirect), 0, 3 }
            : new[] { (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 3,
                (int)(take ? AcsPcode.TakeInventory : AcsPcode.GiveInventory) };
        Run(sim, null, "Clip", words);
        Assert.All(sim.Players, p => Assert.Equal(take ? 47 : 53, p.Inventory.Bullets));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TidZeroBroadcastsActorInventoryOperations(bool take)
    {
        var sim = Room();
        Run(sim, sim.Players.First(), "Clip",
            (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 3,
            (int)(take ? AcsPcode.TakeActorInventory : AcsPcode.GiveActorInventory));
        Assert.All(sim.Players, p => Assert.Equal(take ? 47 : 53, p.Inventory.Bullets));
    }

    [Fact]
    public void TargetedActorGrantHealsNonplayerWithoutBroadcast()
    {
        var sim = Room(); var target = sim.AddBot(200, 0, 3001);
        target.ThingId = 7; target.Health = 20;
        Run(sim, sim.Players.First(), "Medikit",
            (int)AcsPcode.PushNumber, 7, (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 5, (int)AcsPcode.GiveActorInventory);
        Assert.Equal(25, target.Health);
        Assert.All(sim.Players, p => Assert.Equal(100, p.Health));
    }

    [Fact]
    public void MissingNonzeroTidDoesNotBroadcast()
    {
        var sim = Room();
        Run(sim, null, "Clip", (int)AcsPcode.PushNumber, 999,
            (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.GiveActorInventory);
        Assert.All(sim.Players, p => Assert.Equal(50, p.Inventory.Bullets));
    }

    private static void Run(AuthoritySimulation sim, Actor? actor, string type, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = [type], Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, actor));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
