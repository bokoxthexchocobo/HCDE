using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NamedHealthRemovalTests
{
    [Theory]
    [InlineData(2, 3)]
    [InlineData(5, 0)]
    [InlineData(int.MaxValue, 0)]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    public void ScriptRemovalDepletesWithoutHealing(int amount, int remaining)
    {
        var player = Room().Players.First(); player.Health = 40;
        player.Inventory.AddHealthPickup(10, 5, 3, "MedPatch");
        AcsPlayerInventory.Take(player, "MEDPATCH", amount);
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("medpatch"));
        Assert.Equal(40, player.Health);
    }

    [Fact]
    public void ExcessRemovalOnlyAffectsFirstMatchingStack()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(10, 2, typeName: "CustomHealth");
        inventory.AddHealthPickup(25, 3, typeName: "CustomHealth"); inventory.AddHealthPickup(10, 7);
        Assert.Equal(2, inventory.TakeHealthPickup("CUSTOMHEALTH", 10));
        Assert.Equal(new HealthPickupStack(25, 3, 0, "CustomHealth"), inventory.HealthPickups[0]);
        Assert.Equal(7, inventory.HealthPickups[1].Amount);
        Assert.Equal(0, inventory.TakeHealthPickup("MissingHealth", 1));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StackAndDirectOpcodesBroadcastWithMissingActivator(bool direct)
    {
        var sim = Room(); foreach (var player in sim.Players) player.Inventory.AddHealthPickup(-100, 3, typeName: "SurgeryKit");
        var words = direct
            ? new[] { (int)AcsPcode.TakeInventoryDirect, 0, 2 }
            : new[] { (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 2, (int)AcsPcode.TakeInventory };
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = ["SurgeryKit"], Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, null)); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.All(sim.Players, player => Assert.Equal(1, player.Inventory.CountHealthPickup("SurgeryKit")));
        var saved = SimSavegame.Write(sim);
        foreach (var player in sim.Players) player.Inventory.ResetToPistolStart();
        SimSavegame.Apply(sim, saved);
        Assert.All(sim.Players, player => Assert.Equal(1, player.Inventory.CountHealthPickup("SurgeryKit")));
    }

    [Fact]
    public void UnsupportedHealthDropFailsWithoutRemovingInventory()
    {
        var sim = Room(); var player = sim.Players.First(); player.Inventory.AddHealthPickup(10, 2, 3, "MedPatch");
        var before = sim.Actors.Count;
        Assert.False(AcsPlayerInventory.Drop(player, "MedPatch"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch")); Assert.Equal(before, sim.Actors.Count);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
