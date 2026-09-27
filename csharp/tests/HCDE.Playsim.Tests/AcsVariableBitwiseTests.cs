using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsVariableBitwiseTests
{
    public static IEnumerable<object[]> Operations()
    {
        // Native wire opcodes, independent of the enum being tested.
        foreach (var scope in new[] { (0, 25, 28, 19), (1, 26, 29, 127), (3, 181, 182, 63) })
        {
            yield return [291 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, unchecked((int)0x80000000)];
            yield return [298 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, 15];
            yield return [305 + scope.Item1, scope.Item2, scope.Item3, scope.Item4, unchecked((int)0x8000000F)];
        }
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public void OperationUsesAllBitsAndConsumesOnlyItsOperand(int opcode, int assign, int push, int index, int expected)
    {
        var sim = Room();
        Run(sim, 1, 3, unchecked((int)0x80000005), assign, index,
            3, 7, 3, unchecked((int)0x8000000A), opcode, index,
            push, index, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public void IndexBeyondScopeStopsBeforeMutation(int opcode, int assign, int push, int index, int expected)
    {
        _ = assign; _ = push; _ = expected;
        var sim = Room(); Run(sim, 1, 3, 1, opcode, index + 1, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public void MissingOperandStopsAndPreservesSharedValue(int opcode, int assign, int push, int index, int expected)
    {
        _ = expected;
        var sim = Room(); Run(sim, 1, 3, 19, assign, index, opcode, index, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
        if (assign != 25)
        {
            Run(sim, 2, 3, 7, push, index, 5, 112, 1);
            Assert.Equal(19, sim.LightOf(0));
        }
    }

    [Theory]
    [InlineData(291)]
    [InlineData(299)]
    [InlineData(308)]
    public void NegativeIndexStopsBeforeMutation(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 1, opcode, -1, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(291)]
    [InlineData(299)]
    [InlineData(308)]
    public void TruncatedIndexDoesNotChangeSharedState(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 19, 26, 0, 3, 19, 181, 0, 3, 1, opcode);
        Run(sim, 2, 3, 7, 29, 0, 182, 0, 14, 5, 112, 1);
        Assert.Equal(38, sim.LightOf(0));
    }

    [Fact]
    public void ScopesAreIndependentAndSharedUpdatesSurviveScriptTermination()
    {
        var sim = Room();
        Run(sim, 1, 3, 1, 25, 0, 3, 2, 26, 0, 3, 4, 181, 0,
            3, 8, 305, 0, 3, 16, 306, 0, 3, 32, 308, 0,
            3, 7, 28, 0, 3, 9, 19, 29, 0, 3, 18, 19, 14, 182, 0, 3, 36, 19, 14, 5, 112, 1);
        Assert.Equal(3, sim.LightOf(0));
        Run(sim, 2, 3, 7, 28, 0, 29, 0, 14, 182, 0, 14, 5, 112, 1);
        Assert.Equal(54, sim.LightOf(0)); // New locals are zero; map/global flags persist in this VM.
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
