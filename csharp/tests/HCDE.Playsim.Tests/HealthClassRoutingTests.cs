using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthClassRoutingTests
{
    [Theory]
    [InlineData("Stimpack", 47, 0)]
    [InlineData("Medikit", 47, 0)]
    [InlineData("Megasphere", 200, 200)]
    public void DoomClassGrantKeepsItsEffectsInStrifeMapping(string name, int expectedHealth, int expectedArmor)
    {
        foreach (var strife in new[] { false, true })
        {
            var player = Room(strife).Players.Single(); player.Health = 40;
            AcsPlayerInventory.Give(player, name, 7);
            Assert.Equal(expectedHealth, player.Health); Assert.Equal(expectedArmor, player.Inventory.Armor);
            Assert.Empty(player.Inventory.HealthPickups);
        }
    }

    [Fact]
    public void ClassGrantAndMapPickupCoexistWithoutChangingStoredInventory()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.Health = 40;
        AcsPlayerInventory.Give(player, "MedPatch", 2);
        AcsPlayerInventory.Give(player, "Stimpack", 10);
        Assert.Equal(50, player.Health); Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.True(PickupCatalog.TryGive(player, 2011));
        Assert.Equal(50, player.Health); Assert.Equal(3, player.Inventory.CountHealthPickup("MedPatch"));
        var archive = SimSavegame.Write(sim); SimSavegame.Apply(sim, archive);
        Assert.Equal(50, player.Health); Assert.Equal(3, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StackAndDirectScriptGrantsUseClassIdentity(bool direct)
    {
        var sim = Room(true); var player = sim.Players.Single(); player.Health = 40;
        var words = direct ? new[] { (int)AcsPcode.GiveInventoryDirect, 0, 7 }
            : new[] { (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 7, (int)AcsPcode.GiveInventory };
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = ["sTiMpAcK"], Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, player)); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(47, player.Health);
        Assert.Empty(player.Inventory.HealthPickups);
    }

    private static AuthoritySimulation Room(bool strife) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(StrifeHealthPickups: strife));
}
