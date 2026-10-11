using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeHealthGrantTests
{
    [Theory]
    [InlineData("MedPatch", 10, 20, 3)]
    [InlineData("MedicalKit", 25, 15, 3)]
    [InlineData("SurgeryKit", -100, 5, 0)]
    public void ScriptGrantUsesNativeDefaultsAndCapsWithoutHealing(string name, int health, int maximum, int mode)
    {
        var player = Room().Players.Single(); player.Health = 40;
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name.ToLowerInvariant(), int.MaxValue);
        Assert.Equal(new HealthPickupStack(health, maximum, mode, name), Assert.Single(player.Inventory.HealthPickups));
        Assert.Equal(40, player.Health); Assert.False(player.Inventory.GiveHealthPickup(name, 1));
        AcsPlayerInventory.Take(player, name, 2); AcsPlayerInventory.Give(player, name, 1);
        Assert.Equal(maximum - 1, player.Inventory.CountHealthPickup(name));
    }

    [Fact]
    public void GrantsMergeMatchingHealthAndPrependNewStacks()
    {
        var player = Room().Players.Single();
        player.Inventory.AddHealthPickup(99, 1, 3, "MedPatch");
        Assert.True(player.Inventory.GiveHealthPickup("medpatch", 2));
        Assert.True(player.Inventory.GiveHealthPickup("MEDPATCH", 3));
        Assert.Equal(2, player.Inventory.HealthPickups.Count);
        Assert.Equal(new HealthPickupStack(10, 5, 3, "MedPatch"), player.Inventory.HealthPickups[0]);
        Assert.True(player.Inventory.GiveHealthPickup("MedicalKit", 1));
        Assert.Equal("MedicalKit", player.Inventory.HealthPickups[0].TypeName);
    }

    [Fact]
    public void GrantedItemsReachAutomaticAndManualUseAfterRestore()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 60;
        AcsPlayerInventory.Give(player, "MedicalKit", 2); AcsPlayerInventory.Give(player, "SurgeryKit", 1);
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        ActorDamage.Apply(player, 20); Assert.Equal(65, player.Health);
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedicalKit"));
        Assert.Equal(1, AcsPlayerInventory.Use(player, "SurgeryKit")); Assert.Equal(100, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptOpcodeFormsGrantStoredItems(bool direct)
    {
        var sim = Room(); var player = sim.Players.Single();
        var words = direct ? new[] { (int)AcsPcode.GiveInventoryDirect, 0, 3 }
            : new[] { (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 3, (int)AcsPcode.GiveInventory };
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = ["MedPatch"], Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, player)); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(3, player.Inventory.CountHealthPickup("MedPatch"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    });
}
