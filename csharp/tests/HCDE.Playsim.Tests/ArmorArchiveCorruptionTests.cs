using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArmorArchiveCorruptionTests
{
    [Theory]
    [InlineData(44, -1, "save-spare-armor-name")]
    [InlineData(44, 0, "save-spare-armor-name")]
    [InlineData(44, int.MaxValue, "save-spare-armor-name")]
    [InlineData(40, -1, "save-spare-armor-count")]
    [InlineData(40, int.MaxValue, "save-spare-armor-count")]
    [InlineData(65, 2, "save-spare-armor-flags")]
    public void MalformedSpareRecordIsRejected(int offset, int value, string expected)
    {
        var bytes = SpareSave(); var start = TrailerStart(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void InvalidUtf8SpareTypeIsRejected()
    {
        var bytes = SpareSave(); bytes[TrailerStart(bytes) + 48] = 0xff;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-spare-armor-encoding", error);
    }

    [Fact]
    public void EmptySpareTypeCannotProduceUnreadableSave()
    {
        var sim = Room(); sim.Players.Single().Inventory.RestoreSpareArmor([new(100, 33, 0, 0, "")]);
        Assert.Throws<ArgumentException>(() => SimSavegame.Write(sim));
    }

    [Fact]
    public void UnnamedPlayerArmorAmountSurvivesSave()
    {
        var sim = Room(); sim.Players.Single().Inventory.Armor = 42;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.Equal(42, restored.Players.Single().Inventory.Armor);
        Assert.Equal("None", restored.Players.Single().Inventory.ArmorType);
    }

    private static int TrailerStart(byte[] bytes) => bytes.Length
        - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static byte[] SpareSave()
    {
        var sim = Room(); sim.Players.Single().Inventory.RestoreSpareArmor([new(100, 33, 0, 0, "A")]);
        return SimSavegame.Write(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
