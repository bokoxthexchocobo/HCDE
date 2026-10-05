using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPowerInventoryDispatchTests
{
    [Theory]
    [InlineData("PowerDamage", 875)]
    [InlineData("PowerProtection", 875)]
    [InlineData("PowerBuddha", 2100)]
    public void ActorGrantAndTakeTargetTid(string name, int duration)
    {
        var sim = Room();
        Run(sim, sim.Players.First(), name,
            (int)AcsPcode.PushNumber, 9, (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 1, (int)AcsPcode.GiveActorInventory);
        Assert.Equal(0, AcsActorPowerups.RemainingTics(sim.Players.First(), name));
        Assert.Equal(duration, AcsActorPowerups.RemainingTics(sim.Players.Last(), name));
        Run(sim, sim.Players.First(), name,
            (int)AcsPcode.PushNumber, 9, (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 1, (int)AcsPcode.TakeActorInventory);
        Assert.Equal(0, AcsActorPowerups.RemainingTics(sim.Players.Last(), name));
    }

    [Theory]
    [InlineData("PowerDamage")]
    [InlineData("PowerProtection")]
    [InlineData("PowerBuddha")]
    public void DirectGrantAndInventoryQueryExecuteInVm(string name)
    {
        var sim = Room();
        Run(sim, sim.Players.First(), name,
            (int)AcsPcode.GiveInventoryDirect, 0, 1,
            (int)AcsPcode.CheckInventoryDirect, 0,
            (int)AcsPcode.PushNumber, 1, (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(1, AcsPlayerInventory.Count(sim.Players.First(), name, false));
        Assert.Equal(0, AcsPlayerInventory.Count(sim.Players.Last(), name, false));
    }

    [Fact]
    public void BuddhaGrantProtectsUntilTaken()
    {
        var sim = Room(); var player = sim.Players.First();
        Run(sim, player, "PowerBuddha", (int)AcsPcode.GiveInventoryDirect, 0, 1);
        ActorDamage.Apply(player, 1000);
        Assert.Equal(1, player.Health);
        Run(sim, player, "PowerBuddha", (int)AcsPcode.TakeInventoryDirect, 0, 1);
        Assert.True(ActorDamage.Apply(player, 10).Killed);
    }

    private static void Run(AuthoritySimulation sim, Actor activator, string name, params int[] words)
    {
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        var number = sim.Acs.RunningCount + 100;
        sim.Acs.Add(new AcsProgram { Number = number, Code = code, StringTable = [name] });
        Assert.True(sim.Acs.Enqueue(number, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, Id = 9, X = 400 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
