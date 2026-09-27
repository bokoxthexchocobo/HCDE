using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsArgumentTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 19)]
    [InlineData(2, 42)]
    [InlineData(3, 42)]
    public void ArgumentsPopulateDeclaredLocalsAndMissingValuesAreZero(int supplied, int expected)
    {
        var sim = Room(); sim.Acs.Add(Program(2, 3, 7, 28, 0, 28, 1, 14, 5, 112, 1));
        int[] args = [19, 23, 99]; Assert.True(sim.Acs.Enqueue(1, args.AsSpan(0, supplied))); sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ExistingNoArgumentApiZeroInitializesDeclaredArguments()
    {
        var sim = Room(); sim.Acs.Add(Program(1, 3, 7, 28, 0, 5, 112, 1));
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void ZeroArgumentProgramIgnoresSuppliedValues()
    {
        var sim = Room(); sim.Acs.Add(Program(0, 3, 7, 28, 0, 5, 112, 1));
        Assert.True(sim.Acs.Enqueue(1, [19])); sim.Acs.Tick(sim); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void ArgumentsAreCopiedAndInstancesStayIndependentAcrossDelay()
    {
        var sim = Room(); sim.Acs.Add(Program(1, 56, 1, 3, 7, 28, 0, 5, 110, 1));
        int[] args = [19]; Assert.True(sim.Acs.Enqueue(1, args)); args[0] = 23; Assert.True(sim.Acs.Enqueue(1, args)); args[0] = 99;
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(170, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ArgumentsCannotSpillIntoArrayStorage()
    {
        var sim = Room(); var code = Program(0, 3, 7, 28, 0, 3, 0, 365, 0, 14, 5, 112, 1).Code;
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code, ArgumentCount = 1, LocalVariableCount = 1, LocalArraySizes = [1] });
        Assert.True(sim.Acs.Enqueue(1, [19, 99])); sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void InvalidArgumentDeclarationDoesNotReplaceProgram(int count)
    {
        var sim = Room(); sim.Acs.Add(Program(1, 1)); var checksum = sim.Acs.Checksum;
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.Acs.Add(Program(count, 1)));
        Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Fact]
    public void MissingProgramDoesNotCreateFiberOrChangeChecksum()
    {
        var sim = Room(); var checksum = sim.Acs.Checksum;
        Assert.False(sim.Acs.Enqueue(999, [19])); Assert.Equal(checksum, sim.Acs.Checksum); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void DeclaredAndActualArgumentsParticipateInChecksums()
    {
        var a = Room(); var b = Room(); a.Acs.Add(Program(1, 1)); b.Acs.Add(Program(2, 1));
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
        b.Acs.Add(Program(1, 1)); Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
        Assert.True(a.Acs.Enqueue(1, [19])); Assert.True(b.Acs.Enqueue(1, [23])); Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    [Fact]
    public void IgnoredExtraArgumentsDoNotChangeExecutionStateChecksum()
    {
        var a = Room(); var b = Room(); a.Acs.Add(Program(1, 1)); b.Acs.Add(Program(1, 1));
        Assert.True(a.Acs.Enqueue(1, [19])); Assert.True(b.Acs.Enqueue(1, [19, 23])); Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
    }

    [Fact]
    public void ReplacementDoesNotChangeQueuedArguments()
    {
        var sim = Room(); sim.Acs.Add(Program(1, 3, 7, 28, 0, 5, 112, 1));
        Assert.True(sim.Acs.Enqueue(1, [19])); sim.Acs.Add(Program(0, 1)); sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    private static AcsProgram Program(int count, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = 1, Code = bytes, ArgumentCount = count };
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
