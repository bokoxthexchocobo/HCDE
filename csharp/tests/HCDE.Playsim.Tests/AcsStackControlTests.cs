using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsStackControlTests
{
    [Fact]
    public void DuplicatePreservesLowerStackAndAddsAnotherTopValue()
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, 19, 216, 14, 5, 112, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(38, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void SwapReversesOnlyTheTopTwoValues()
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, 10, 3, 30, 217, 15, 5, 112, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(20, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(-1, 0)]
    [InlineData(int.MinValue, int.MaxValue)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(0x55555555, unchecked((int)0xAAAAAAAA))]
    public void BitwiseNegationUsesAllThirtyTwoBits(int input, int expected)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, input, 330, 3, expected, 19, 5, 112, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(216)]
    [InlineData(217)]
    [InlineData(330)]
    public void EmptyStackStopsBeforeMutation(int opcode)
    {
        var sim = Room(); Add(sim, 1, opcode, 10, 112, 7, 35, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void SwapRequiresTwoValues()
    {
        var sim = Room(); Add(sim, 1, 3, 7, 217, 10, 112, 7, 35, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(196)]
    [InlineData(197)]
    [InlineData(333)]
    public void FormerEnumValuesDoNotExecuteOldStackOperations(int opcode)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, 35, opcode, 5, 112, 1);
        sim.Acs.Tick(sim);
        // X/Y return zero for missing TID 35; native PrintBind (333) remains unsupported.
        Assert.Equal(opcode == 333 ? 128 : 0, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void RestartRetainsLocalsAndStackWithinExecution()
    {
        // Retain each iteration's counter on the stack, then add 1 + 2 + 3.
        var sim = Room(); Add(sim, 1, 46, 0, 28, 0, 216, 3, 3, 24, 53, 44, 69,
            14, 14, 3, 7, 217, 5, 112, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(6, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void RestartAndDelayRetainLocalCounterAcrossTicks()
    {
        var sim = Room(); Add(sim, 1, 46, 0, 28, 0, 3, 3, 24, 53, 48, 56, 1, 69,
            3, 7, 28, 0, 5, 112, 1);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(3, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void InfiniteRestartYieldsBudgetAndDoesNotStarveAnotherScript()
    {
        var sim = Room(); Add(sim, 1, 69); Add(sim, 2, 10, 112, 7, 35, 1);
        sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(1, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
        Assert.True(sim.Acs.Enqueue(number));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
