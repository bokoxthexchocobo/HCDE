using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLocalVariableTests
{
    [Theory]
    [InlineData(25, 3)]
    [InlineData(31, 10)]
    [InlineData(34, 4)]
    [InlineData(37, 21)]
    [InlineData(40, 2)]
    [InlineData(43, 1)]
    public void LocalArithmeticUsesNativeOpcodesAndLastDefaultSlot(int opcode, int expected)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 25, 19, 3, 3, opcode, 19, 3, 7, 28, 19, 5, 112, 1);
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void ConcurrentFibersKeepIndependentLocalsAcrossDelay()
    {
        var sim = Room();
        Add(sim, 1, 3, 40, 25, 0, 56, 2, 3, 7, 28, 0, 5, 112, 1);
        Add(sim, 2, 3, 80, 25, 0, 56, 1, 3, 7, 28, 0, 5, 112, 1);
        sim.Acs.Enqueue(1); sim.Acs.Enqueue(2);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(80, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(40, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void EachInvocationStartsZeroAndIncrementDecrementNeedNoStack()
    {
        var sim = Room(); Add(sim, 1, 46, 0, 46, 0, 49, 0, 3, 7, 28, 0, 5, 112, 1);
        for (var i = 0; i < 2; i++) { sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0)); }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(20)]
    public void InvalidLocalIndexStopsBeforeMutation(int index)
    {
        var sim = Room(); Add(sim, 1, 3, 35, 25, index, 10, 112, 7, 35, 1);
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(40)]
    [InlineData(43)]
    public void ZeroDivisorStopsLocalArithmetic(int opcode)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 25, 0, 3, 0, opcode, 0, 10, 112, 7, 35, 1);
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
