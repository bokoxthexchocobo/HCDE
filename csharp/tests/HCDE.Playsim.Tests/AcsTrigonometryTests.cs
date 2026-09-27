using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTrigonometryTests
{
    [Theory]
    [InlineData(220, 0, 0)]
    [InlineData(220, 16384, 65536)]
    [InlineData(220, 32768, 0)]
    [InlineData(220, 49152, -65536)]
    [InlineData(220, -16384, -65536)]
    [InlineData(220, 81920, 65536)]
    [InlineData(220, 8192, 46341)]
    [InlineData(220, int.MaxValue, -6)]
    [InlineData(221, 0, 65536)]
    [InlineData(221, 16384, 0)]
    [InlineData(221, 32768, -65536)]
    [InlineData(221, 49152, 0)]
    [InlineData(221, -8192, 46341)]
    [InlineData(221, int.MinValue, 65536)]
    public void TrigUsesFixedPointTurnsAndFixedPointResult(int opcode, int angle, int expected)
    {
        AssertQuery([3, angle, opcode], expected);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 16384)]
    [InlineData(-1, 0, 32768)]
    [InlineData(0, -1, 49152)]
    [InlineData(1, 1, 8192)]
    [InlineData(-1, 1, 24576)]
    [InlineData(-1, -1, 40960)]
    [InlineData(1, -1, 57344)]
    [InlineData(0, 0, 0)]
    [InlineData(2, 1, 4836)]
    [InlineData(2000, 1000, 4836)]
    [InlineData(int.MinValue, int.MinValue, 40960)]
    public void VectorAngleUsesXYOrderAndTruncatesUnsignedTurns(int x, int y, int expected)
    {
        AssertQuery([3, x, 3, y, 222], expected);
    }

    [Theory]
    [InlineData(220, 0)]
    [InlineData(221, 0)]
    [InlineData(222, 0)]
    [InlineData(222, 1)]
    public void MissingArgumentsStopFollowingAction(int opcode, int supplied)
    {
        var sim = Room();
        Run(sim, supplied == 0 ? [opcode, 10, 112, 7, 35, 1] : [3, 1, opcode, 10, 112, 7, 35, 1]);
        Assert.Equal(128, sim.LightOf(0));
    }

    private static void AssertQuery(int[] expression, int expected)
    {
        var sim = Room();
        Run(sim, [3, 7, .. expression, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }
    private static void Run(AuthoritySimulation sim, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
