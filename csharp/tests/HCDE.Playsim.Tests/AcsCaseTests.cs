using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsCaseTests
{
    [Theory]
    [InlineData(-1, 35)]
    [InlineData(2, 70)]
    [InlineData(3, 99)]
    public void CaseChainRetainsSelectorUntilMatchOrDefault(int selector, int expected)
    {
        var sim = Room();
        Run(sim, 3, 7, 3, selector, 84, -1, 68, 84, 2, 92,
            54, 3, 99, 5, 112, 1, 0,
            3, 35, 5, 112, 1, 0,
            3, 70, 5, 112, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-10, 35)]
    [InlineData(10, 70)]
    [InlineData(0, 99)]
    public void SortedCasesConsumeOnlyMatchedSelector(int selector, int expected)
    {
        var sim = Room();
        Run(sim, 3, 7, 3, selector, 256, 2, -10, 68, 10, 92,
            54, 3, 99, 5, 112, 1, 0,
            3, 35, 5, 112, 1, 0,
            3, 70, 5, 112, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void EmptySortedTablePreservesSelector()
    {
        var sim = Room(); Run(sim, 3, 7, 3, 35, 256, 0, 5, 112, 1);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(2)]
    public void InvalidTableLengthStopsBeforeFollowingMutation(int count)
    {
        var sim = Room(); Run(sim, 3, 7, 256, count, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-4)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void InvalidTakenBranchStopsScript(int destination)
    {
        var sim = Room(); Run(sim, 3, 1, 84, 1, destination, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void MissingSelectorStopsBeforeCaseMutation()
    {
        var sim = Room(); Run(sim, 84, 0, 12, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(52, -4)]
    [InlineData(52, 1)]
    [InlineData(52, int.MaxValue)]
    [InlineData(53, -4)]
    [InlineData(53, 1)]
    [InlineData(53, int.MaxValue)]
    [InlineData(79, -4)]
    [InlineData(79, 1)]
    [InlineData(79, int.MaxValue)]
    public void OrdinaryTakenBranchesRejectInvalidDestinations(int opcode, int destination)
    {
        var sim = Room();
        Run(sim, 3, opcode == 79 ? 0 : 1, opcode, destination, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(53, 0)]
    [InlineData(79, 1)]
    public void UntakenConditionalDoesNotValidateUnusedDestination(int opcode, int condition)
    {
        var sim = Room();
        Run(sim, 3, condition, opcode, -4, 10, 112, 7, 35, 1);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }], Format = MapDataFormat.HexenBinary });
}
