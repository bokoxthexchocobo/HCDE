using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsVariableShiftTests
{
    public static IEnumerable<object[]> Shifts()
    {
        foreach (var scope in new[] { (0, 25, 28, 19), (1, 26, 29, 127), (3, 181, 182, 63) })
        {
            yield return [312 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, 3, 4, 48];
            yield return [312 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, 1, 31, int.MinValue];
            yield return [312 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, 17, 32, 17];
            yield return [319 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, -9, 1, -5];
            yield return [319 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, int.MinValue, 31, -1];
            yield return [319 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, int.MaxValue, -1, 0];
        }
    }

    [Theory]
    [MemberData(nameof(Shifts))]
    public void ScalarShiftsPreserveScopeAndLowerStack(int opcode, int assign, int push, int index, int value, int count, int expected)
    {
        var sim = Room(); Run(sim, 1, 3, value, assign, index, 3, 7, 3, count, opcode, index,
            push, index, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(312, 20)]
    [InlineData(313, 128)]
    [InlineData(315, 64)]
    [InlineData(319, -1)]
    [InlineData(320, -1)]
    [InlineData(322, -1)]
    public void InvalidIndexStopsBeforeMutation(int opcode, int index)
    {
        var sim = Room(); Run(sim, 1, 3, 1, opcode, index, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(312)]
    [InlineData(313)]
    [InlineData(315)]
    [InlineData(319)]
    [InlineData(320)]
    [InlineData(322)]
    public void UnderflowPreservesSharedVariables(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 17, 26, 0, 3, 23, 181, 0, opcode, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
        Run(sim, 2, 3, 7, 29, 0, 182, 0, 14, 5, 112, 1);
        Assert.Equal(40, sim.LightOf(0));
    }

    [Theory]
    [InlineData(312)]
    [InlineData(320)]
    [InlineData(322)]
    public void MissingIndexPreservesSharedVariables(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 17, 26, 0, 3, 23, 181, 0, 3, 1, opcode);
        Run(sim, 2, 3, 7, 29, 0, 182, 0, 14, 5, 112, 1);
        Assert.Equal(40, sim.LightOf(0));
    }

    [Fact]
    public void ScopesStaySeparateAndSharedShiftsPersistAcrossScripts()
    {
        var sim = Room(); Run(sim, 1, 3, 3, 25, 0, 3, 5, 26, 0, 3, 7, 181, 0,
            3, 1, 312, 0, 3, 2, 313, 0, 3, 3, 315, 0,
            3, 7, 28, 0, 29, 0, 14, 182, 0, 14, 5, 112, 1);
        Assert.Equal(82, sim.LightOf(0));
        Run(sim, 2, 3, 7, 28, 0, 29, 0, 14, 182, 0, 14, 5, 112, 1);
        Assert.Equal(76, sim.LightOf(0));
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
