using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLocalArrayTests
{
    [Fact]
    public void DecodedLayoutCanBeUsedForScriptExecution()
    {
        byte[] chunks = [83, 86, 67, 84, 4, 0, 0, 0, 1, 0, 3, 0,
            83, 65, 82, 89, 6, 0, 0, 0, 1, 0, 2, 0, 0, 0];
        Assert.True(MapBehaviorLocalLayoutCodec.TryReadChunks(chunks, out var layouts, out var error), error);
        var layout = layouts![1]; var sim = Room();
        var code = Program([], 3, 19, 25, 4, 3, 7, 3, 1, 365, 0, 5, 112, 1).Code;
        Run(sim, new AcsProgram { Number = 1, Code = code, LocalVariableCount = layout.LocalVariableCount, LocalArraySizes = layout.ArraySizes.ToArray() });
        Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(364, 3)]
    [InlineData(366, 10)]
    [InlineData(367, 4)]
    [InlineData(368, 21)]
    [InlineData(369, 2)]
    [InlineData(370, 1)]
    [InlineData(373, 3)]
    [InlineData(374, 4)]
    [InlineData(375, 7)]
    [InlineData(376, 56)]
    [InlineData(377, 0)]
    public void LocalArrayOperationsPreserveLowerStack(int opcode, int expected)
    {
        var sim = Room(); Run(sim, Program([2], 3, 1, 3, 7, 364, 0,
            3, 7, 3, 1, 3, 3, opcode, 0, 3, 1, 365, 0, 5, 112, 1));
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void ArraysUseDeclaredOffsetsInLocalStorage()
    {
        var sim = Room(); Run(sim, Program([0, 2, 1], 3, 19, 25, 20,
            3, 0, 3, 23, 364, 2, 3, 1, 371, 1, 3, 1, 371, 1, 3, 1, 372, 1,
            3, 7, 3, 0, 365, 1, 28, 22, 14, 3, 1, 365, 1, 14, 5, 112, 1));
        Assert.Equal(43, sim.LightOf(0)); // 19 + 23 + 1, through shared scalar/array storage.
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(1, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 2)]
    public void InvalidArrayOrElementReadsZeroAndIgnoresWrites(int id, int index)
    {
        var sim = Room(); Run(sim, Program([2], 3, index, 3, 19, 364, id,
            3, 7, 3, index, 365, id, 5, 112, 1));
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(369)]
    [InlineData(370)]
    public void ZeroDivisorStopsWithoutChangingArray(int opcode)
    {
        var sim = Room(); var program = Program([1], 3, 0, 3, 19, 364, 0,
            3, 0, 3, 0, opcode, 0, 10, 112, 7, 35, 1);
        Run(sim, program); Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(364)]
    [InlineData(365)]
    [InlineData(371)]
    [InlineData(377)]
    public void MissingStackArgumentsStopBeforeMutation(int opcode)
    {
        var sim = Room(); Run(sim, Program([1], opcode, 0, 10, 112, 7, 35, 1));
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void ConcurrentInstancesHaveIndependentArraysAcrossDelay()
    {
        var sim = Room(); var program = Program([1], 3, 0, 371, 0, 56, 1,
            3, 7, 3, 0, 365, 0, 5, 110, 1); // Raise light by this instance's counter.
        sim.Acs.Add(program); Assert.True(sim.Acs.Enqueue(1)); Assert.True(sim.Acs.Enqueue(1));
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(2, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(130, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void RegisteredLayoutIsOwnedAndActiveInstanceSurvivesReplacement()
    {
        var sim = Room(); var sizes = new[] { 2 };
        sim.Acs.Add(Program(sizes, 3, 1, 3, 19, 364, 0, 56, 1, 3, 7, 3, 1, 365, 0, 5, 112, 1));
        sizes[0] = 0; Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim);
        sim.Acs.Add(Program([], 1)); sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void LocalVariableCountControlsArrayStart()
    {
        var sim = Room(); var code = Program([], 3, 19, 25, 3, 3, 7, 3, 0, 365, 0, 5, 112, 1).Code;
        Run(sim, new AcsProgram { Number = 1, Code = code, LocalVariableCount = 3, LocalArraySizes = [1] });
        Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(20, -1)]
    [InlineData(20, int.MaxValue)]
    [InlineData(int.MaxValue, 0)]
    public void InvalidLayoutDoesNotReplaceRegisteredProgram(int locals, int size)
    {
        var sim = Room(); sim.Acs.Add(Program([1], 1)); var checksum = sim.Acs.Checksum;
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.Acs.Add(new AcsProgram
        { Number = 1, Code = [1, 0, 0, 0], LocalVariableCount = locals, LocalArraySizes = [size] }));
        Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Fact]
    public void LayoutParticipatesInChecksumEvenWithEqualTotalStorage()
    {
        var a = Room(); var b = Room(); a.Acs.Add(Program([1, 2], 56, 1, 1)); b.Acs.Add(Program([2, 1], 56, 1, 1));
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
        Assert.True(a.Acs.Enqueue(1)); Assert.True(b.Acs.Enqueue(1));
        a.Acs.Add(Program([], 1)); b.Acs.Add(Program([], 1));
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum); // Active layouts remain distinguishable.
    }

    private static AcsProgram Program(int[] sizes, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = 1, Code = bytes, LocalArraySizes = sizes };
    }
    private static void Run(AuthoritySimulation sim, AcsProgram program)
    { sim.Acs.Add(program); Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
