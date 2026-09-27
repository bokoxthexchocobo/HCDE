using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMapVariableTests
{
    [Theory]
    [InlineData(26, 3)]
    [InlineData(32, 10)]
    [InlineData(35, 4)]
    [InlineData(38, 21)]
    [InlineData(41, 2)]
    [InlineData(44, 1)]
    public void MapArithmeticUsesLastSlotAndPersistsBetweenScripts(int opcode, int expected)
    {
        var sim = Room(); Run(sim, 1, 3, 7, 26, 127, 3, 3, opcode, 127, 1);
        Run(sim, 2, 3, 7, 29, 127, 5, 112, 1); Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void MapLocalAndGlobalSlotsRemainDistinct()
    {
        var sim = Room();
        Run(sim, 1, 3, 10, 26, 0, 3, 20, 181, 0, 3, 30, 25, 0,
            3, 7, 29, 0, 182, 0, 14, 28, 0, 14, 5, 112, 1);
        Assert.Equal(60, sim.LightOf(0));
    }

    [Fact]
    public void NewSimulationStartsWithZeroMapVariables()
    {
        var sim = Room(); Run(sim, 1, 47, 0, 47, 0, 50, 0, 3, 7, 29, 0, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
        var other = Room(); Run(other, 1, 3, 7, 29, 0, 5, 112, 1); Assert.Equal(0, other.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(128)]
    public void InvalidIndexStopsBeforeLaterAction(int index)
    {
        var sim = Room(); Run(sim, 1, 3, 7, 26, index, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(41)]
    [InlineData(44)]
    public void ZeroDivisorLeavesMapVariableUnchanged(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 7, 26, 0, 3, 0, opcode, 0, 47, 0, 1);
        Run(sim, 2, 3, 7, 29, 0, 5, 112, 1); Assert.Equal(7, sim.LightOf(0));
    }

    [Fact]
    public void MapValuesAffectChecksumWithIdenticalProgramRegistry()
    {
        var a = Room(); var b = Room(); Run(a, 1, 3, 10, 26, 0, 1); Run(b, 1, 3, 20, 26, 0, 1);
        Run(a, 1, 1); Run(b, 1, 1); // Replace registration, retaining only the differing variable values.
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
    { Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }], Format = MapDataFormat.HexenBinary });
}
