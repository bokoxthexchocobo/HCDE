using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArmorTypeSaveTests
{
    [Theory]
    [InlineData("GreenArmor", 50)]
    [InlineData("BlueArmor", 0)]
    [InlineData("CustomArmor", 25)]
    public void WornTypeSurvivesBinarySaveIncludingDepletion(string type, int amount)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Armor = amount; player.Inventory.ArmorType = type;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(63, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.Equal(type, restored.Players.Single().Inventory.ArmorType);
        Assert.Equal(2, ActorJumpActions.JumpIfArmorType(restored.Players.Single(), type, 2, amount));
        Assert.Equal(bytes, SimSavegame.Write(restored));
    }

    [Fact]
    public void LegacySaveClearsRuntimeArmorType()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        sim.Players.Single().Inventory.ArmorType = "GreenArmor";
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Equal("None", sim.Players.Single().Inventory.ArmorType);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 63, "save-armor-type-header")]
    [InlineData(4, int.MaxValue, "save-armor-type-header")]
    [InlineData(8, -1, "save-armor-type-name")]
    public void CorruptTrailerIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); sim.Players.Single().Inventory.ArmorType = "GreenArmor";
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expected, error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
