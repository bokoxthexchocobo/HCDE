using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsFixedPointTests
{
    [Theory]
    [InlineData(136, 98304, 147456, 221184)] // 1.5 * 2.25 = 3.375
    [InlineData(136, -98304, 147456, -221184)]
    [InlineData(136, -98304, -147456, 221184)]
    [InlineData(136, 0, int.MaxValue, 0)]
    [InlineData(136, -1, 32768, -1)] // Arithmetic shift rounds negative products downward.
    [InlineData(136, 1, 32768, 0)]
    [InlineData(136, int.MaxValue, 131072, -2)] // Multiplication wraps rather than saturating.
    [InlineData(136, int.MinValue, -65536, int.MinValue)]
    [InlineData(137, 196608, 131072, 98304)] // 3 / 2 = 1.5
    [InlineData(137, -196608, 131072, -98304)]
    [InlineData(137, 196608, -131072, -98304)]
    [InlineData(137, -196608, -131072, 98304)]
    [InlineData(137, 65536, 196608, 21845)]
    [InlineData(137, -65536, 196608, -21845)] // Division truncates toward zero.
    [InlineData(137, 0, 65536, 0)]
    [InlineData(137, 32767, 1, 2147418112)]
    [InlineData(137, 32768, 1, int.MaxValue)]
    [InlineData(137, -32768, 1, int.MinValue)]
    [InlineData(137, 32768, -1, int.MinValue)]
    [InlineData(137, int.MinValue, int.MinValue, 65536)]
    [InlineData(137, int.MaxValue, 65536, int.MaxValue)]
    [InlineData(137, int.MinValue, -65536, int.MaxValue)]
    [InlineData(137, 1, 0, int.MaxValue)]
    [InlineData(137, -1, 0, int.MinValue)]
    [InlineData(137, 0, 0, int.MaxValue)]
    public void NativeFixedPointResultsPreserveLowerStackAndContinue(int opcode, int left, int right, int expected)
    {
        // Compare the entire 32-bit result before using light level as an observable output.
        var sim = Run(3, 7, 3, left, 3, right, opcode, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(136, false)]
    [InlineData(136, true)]
    [InlineData(137, false)]
    [InlineData(137, true)]
    public void MissingOperandsStopBeforeFollowingMutation(int opcode, bool oneOperand)
    {
        int[] words = oneOperand ? [3, 7, opcode, 10, 112, 7, 35, 1] : [opcode, 10, 112, 7, 35, 1];
        Assert.Equal(128, Run(words).LightOf(0));
    }

    [Theory]
    [InlineData(135)] // SINGLEPLAYER, not FIXEDMUL.
    [InlineData(138)] // SETGRAVITY, not arithmetic.
    public void AdjacentUnsupportedOpcodesDoNotExecuteArithmetic(int opcode)
    {
        Assert.Equal(128, Run(3, 65536, 3, 65536, opcode, 10, 112, 7, 35, 1).LightOf(0));
    }

    private static AuthoritySimulation Run(params int[] words)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
        return sim;
    }
}
