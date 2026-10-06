using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MeleeFlagArchiveAuditTests
{
    [Theory]
    [InlineData(0, 62, "save-melee-flag-header")]
    [InlineData(0, 14, "save-melee-flag-header")]
    [InlineData(4, -1, "save-melee-flag-header")]
    [InlineData(4, int.MaxValue, "save-melee-flag-header")]
    [InlineData(8, 2, "save-melee-flag-value")]
    [InlineData(8, -1, "save-melee-flag-value")]
    public void MalformedTrailerIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); sim.Players.Single().NoVerticalMeleeRange = true;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expected, error);
    }

    [Fact]
    public void LegacySaveClearsRuntimeOverrideAndKeepsOriginalFormat()
    {
        var sim = Room(); var original = SimSavegame.Write(sim);
        sim.Players.Single().NoVerticalMeleeRange = true;
        Assert.True(SimSavegame.TryRead(original, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.False(sim.Players.Single().NoVerticalMeleeRange);
        Assert.Equal(original, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
