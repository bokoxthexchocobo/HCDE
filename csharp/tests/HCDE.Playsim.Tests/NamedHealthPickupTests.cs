using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NamedHealthPickupTests
{
    [Fact]
    public void ScriptCountsAndUsesNamedItemsCaseInsensitively()
    {
        var player = Room().Players.Single(); player.Health = 40;
        player.Inventory.AddHealthPickup(10, 2, 3, "MedPatch");
        Assert.Equal(2, AcsPlayerInventory.Count(player, "medpatch", false));
        Assert.Equal(1, AcsPlayerInventory.Use(player, "MEDPATCH"));
        Assert.Equal(50, player.Health); Assert.Equal(1, AcsPlayerInventory.Count(player, "MedPatch", false));
        Assert.Equal(0, AcsPlayerInventory.Use(player, "MissingHealthItem"));
    }

    [Fact]
    public void FailedUseRetainsNamedPercentageItem()
    {
        var player = Room().Players.Single(); player.Inventory.AddHealthPickup(-100, 1, typeName: "SurgeryKit");
        Assert.Equal(0, AcsPlayerInventory.Use(player, "SurgeryKit"));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "SurgeryKit", false));
        player.Health = 20; Assert.Equal(1, AcsPlayerInventory.Use(player, "SurgeryKit"));
        Assert.Equal(100, player.Health); Assert.Equal(0, AcsPlayerInventory.Count(player, "SurgeryKit", false));
    }

    [Fact]
    public void NamedAndAnonymousStacksRoundTripWithoutChangingLegacyFormat()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.AddHealthPickup(10, 2, 3);
        var legacy = SimSavegame.Write(sim); Assert.Equal(162, BinaryPrimitives.ReadUInt16LittleEndian(legacy.AsSpan(4)));
        player.Inventory.AddHealthPickup(25, 2, 3, "MedicalKit");
        var named = SimSavegame.Write(sim); Assert.Equal(163, BinaryPrimitives.ReadUInt16LittleEndian(named.AsSpan(4)));
        player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, named);
        Assert.Null(player.Inventory.HealthPickups[0].TypeName);
        Assert.Equal("MedicalKit", player.Inventory.HealthPickups[1].TypeName);
        Assert.Equal(named, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Null(Assert.Single(player.Inventory.HealthPickups).TypeName);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void FirstMatchingStackIsUsedWithoutMergingDifferentHealingValues()
    {
        var player = Room().Players.Single(); player.Health = 40;
        player.Inventory.AddHealthPickup(10, 1, typeName: "CustomHealth");
        player.Inventory.AddHealthPickup(25, 1, typeName: "CustomHealth");
        Assert.Equal(1, player.Inventory.CountHealthPickup("CustomHealth"));
        Assert.True(player.Inventory.UseHealthPickup("CustomHealth"));
        Assert.Equal(50, player.Health); Assert.Equal(25, Assert.Single(player.Inventory.HealthPickups).Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(100000)]
    public void MalformedNameLengthsAreRejected(int length)
    {
        var sim = Room(); sim.Players.Single().Inventory.AddHealthPickup(10, 1, typeName: "MedPatch");
        var bytes = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 24), length);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-health-pickups-invalid", error);
    }

    [Fact]
    public void ItemIdentityParticipatesInChecksum()
    {
        var first = Room(); var second = Room();
        first.Players.Single().Inventory.AddHealthPickup(10, 1, typeName: "FirstHealth");
        second.Players.Single().Inventory.AddHealthPickup(10, 1, typeName: "SecondHealth");
        SimSavegame.Apply(first, SimSavegame.Write(first)); SimSavegame.Apply(second, SimSavegame.Write(second));
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    });
}
