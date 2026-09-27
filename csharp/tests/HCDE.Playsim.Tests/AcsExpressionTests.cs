using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsExpressionTests
{
    [Theory]
    [InlineData(16, -7, 3, -21)]
    [InlineData(17, -7, 3, -2)]
    [InlineData(18, -7, 3, -1)]
    [InlineData(19, 7, 7, 1)]
    [InlineData(19, 7, 8, 0)]
    [InlineData(20, 7, 8, 1)]
    [InlineData(20, 7, 7, 0)]
    [InlineData(21, -7, 3, 1)]
    [InlineData(21, 3, -7, 0)]
    [InlineData(22, 3, -7, 1)]
    [InlineData(22, -7, 3, 0)]
    [InlineData(23, 3, 3, 1)]
    [InlineData(23, 4, 3, 0)]
    [InlineData(24, 3, 3, 1)]
    [InlineData(24, 2, 3, 0)]
    [InlineData(70, -2, 3, 1)]
    [InlineData(70, -2, 0, 0)]
    [InlineData(71, 0, -2, 1)]
    [InlineData(71, 0, 0, 0)]
    [InlineData(72, 6, 3, 2)]
    [InlineData(73, 6, 3, 7)]
    [InlineData(74, 6, 3, 5)]
    [InlineData(76, 3, 2, 12)]
    [InlineData(77, -8, 2, -2)]
    public void BinaryOperationsPreserveOrderAndProduceExpectedValue(int opcode, int left, int right, int expected)
    {
        var sim = Room(); Run(sim, 3, 7, 3, left, 3, right, opcode, 5, 112, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(75, 0, 1)]
    [InlineData(75, -8, 0)]
    [InlineData(78, 8, -8)]
    public void UnaryOperationsReturnExpectedValue(int opcode, int input, int expected)
    {
        var sim = Room(); Run(sim, 3, 7, 3, input, opcode, 5, 112, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0, 128)]
    [InlineData(1, 35)]
    [InlineData(-1, 35)]
    public void IfNotGotoSkipsMutationOnlyForZero(int condition, int expected)
    {
        var sim = Room(); Run(sim, 3, condition, 79, 32, 10, 112, 7, 35, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(17)]
    [InlineData(18)]
    public void ZeroDivisorStopsBeforeWorldMutation(int opcode)
    {
        var sim = Room(); Run(sim, 3, 7, 3, 0, opcode, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(14)]
    [InlineData(19)]
    [InlineData(70)]
    [InlineData(76)]
    [InlineData(75)]
    [InlineData(78)]
    [InlineData(79)]
    public void UnderflowDoesNotExecuteFollowingMutation(int opcode)
    {
        var sim = Room(); Run(sim, opcode, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
