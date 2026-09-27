using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsWorldVariableTests
{
    [Theory]
    [InlineData(27, 7, 3, 3)]
    [InlineData(33, 7, 3, 10)]
    [InlineData(36, 7, 3, 4)]
    [InlineData(39, -7, 3, -21)]
    [InlineData(42, -7, 3, -2)]
    [InlineData(45, -7, 3, -1)]
    [InlineData(293, 6, 3, 2)]
    [InlineData(300, 6, 3, 5)]
    [InlineData(307, 6, 3, 7)]
    [InlineData(314, 3, 4, 48)]
    [InlineData(321, -9, 1, -5)]
    [InlineData(33, int.MaxValue, 1, int.MinValue)]
    [InlineData(42, int.MinValue, -1, int.MinValue)]
    [InlineData(45, int.MinValue, -1, 0)]
    public void OperationsUseLastWorldSlotAndPersistAcrossScripts(int opcode, int initial, int operand, int expected)
    {
        var sim = Room(); Run(sim, 1, 3, initial, 27, 255, 3, operand, opcode, 255, 1);
        Run(sim, 2, 3, 7, 30, 255, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(48, int.MaxValue, int.MinValue)]
    [InlineData(51, int.MinValue, int.MaxValue)]
    public void IncrementAndDecrementDoNotConsumeStack(int opcode, int initial, int expected)
    {
        var sim = Room(); Run(sim, 1, 3, initial, 27, 255, 3, 7, opcode, 255,
            30, 255, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    [InlineData(int.MaxValue)]
    public void InvalidIndexStopsBeforeLaterMutation(int index)
    {
        var sim = Room(); Run(sim, 1, 3, 7, 27, index, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(42)]
    [InlineData(45)]
    public void ZeroDivisorStopsAndLeavesWorldValueIntact(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 17, 27, 0, 3, 0, opcode, 0, 48, 0, 1);
        Run(sim, 2, 3, 7, 30, 0, 5, 112, 1);
        Assert.Equal(17, sim.LightOf(0));
    }

    [Theory]
    [InlineData(27)]
    [InlineData(33)]
    [InlineData(293)]
    [InlineData(314)]
    public void UnderflowDoesNotModifyWorldSlot(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 17, 27, 0, opcode, 0, 48, 0, 1);
        Run(sim, 2, 3, 7, 30, 0, 5, 112, 1);
        Assert.Equal(17, sim.LightOf(0));
    }

    [Fact]
    public void TruncatedIndexDoesNotModifyWorldSlot()
    {
        var sim = Room(); Run(sim, 1, 3, 17, 27, 0, 3, 19, 27);
        Run(sim, 2, 3, 7, 30, 0, 5, 112, 1); Assert.Equal(17, sim.LightOf(0));
    }

    [Fact]
    public void FourScopesRemainIndependent()
    {
        var sim = Room(); Run(sim, 1, 3, 1, 25, 0, 3, 2, 26, 0, 3, 4, 27, 0, 3, 8, 181, 0,
            3, 7, 28, 0, 3, 1, 19, 29, 0, 3, 2, 19, 14,
            30, 0, 3, 4, 19, 14, 182, 0, 3, 8, 19, 14, 5, 112, 1);
        Assert.Equal(4, sim.LightOf(0));
    }

    [Fact]
    public void SeparateSimulationsDoNotShareWorldStorage()
    {
        var a = Room(); var b = Room(); Run(a, 1, 3, 19, 27, 0, 1);
        Run(b, 1, 3, 7, 30, 0, 5, 112, 1); Assert.Equal(0, b.LightOf(0));
    }

    [Fact]
    public void WorldValuesAffectChecksumWithIdenticalRegistryAndNoFibers()
    {
        var a = Room(); var b = Room(); Run(a, 1, 3, 19, 27, 0, 1); Run(b, 1, 3, 20, 27, 0, 1);
        Run(a, 1, 1); Run(b, 1, 1);
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    private static void Run(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
        Assert.True(sim.Acs.Enqueue(number)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
