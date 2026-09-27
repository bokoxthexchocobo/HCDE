using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsStackLightingTests
{
    [Fact]
    public void ResultCallMissingSpecialDoesNotApplyAction()
    {
        var sim = Room(); Run(sim, 3, 7, 3, 35, 3, 0, 3, 0, 3, 0, 263);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263)]
    [InlineData(382)]
    public void ResultReplacesFiveArgumentsAndPreservesLowerStack(int opcode)
    {
        var sim = Room();
        Run(sim, 3, 7, 3, 7, 3, 35, 3, 0, 3, 0, 3, 0, opcode, 112, 5, 110, 1);
        Assert.Equal(36, sim.LightOf(0)); // Change to 35, then raise by returned success value 1.
    }

    [Fact]
    public void ExtendedVoidCallConsumesAllFiveArgumentsWithoutReturningValue()
    {
        var sim = Room();
        Run(sim, 3, 7, 3, 7, 3, 35, 3, 0, 3, 0, 3, 0, 381, 112, 4, 112, 1);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(381)]
    [InlineData(382)]
    [InlineData(263)]
    public void ExtendedCallRejectsIncompleteStackBeforeApplyingAction(int opcode)
    {
        var sim = Room(); Run(sim, 3, 7, 3, 35, opcode, 112, 1);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(110, 2)]
    [InlineData(111, 2)]
    [InlineData(112, 2)]
    [InlineData(113, 3)]
    [InlineData(114, 4)]
    [InlineData(115, 3)]
    [InlineData(116, 5)]
    [InlineData(117, 1)]
    [InlineData(232, 3)]
    [InlineData(233, 1)]
    [InlineData(234, 1)]
    public void StackArgumentsMatchDirectActionAndFutureSimulation(int special, int count)
    {
        var stack = Room(); var direct = Room();
        var args = new[] { 7, 200, 40, 2, 3 }.Take(count).ToArray();
        var words = new List<int>();
        foreach (var arg in args) words.AddRange([3, arg]);
        words.AddRange([3 + count, special, 1]);
        Run(stack, words.ToArray());
        Run(direct, [8 + count, special, ..args, 1]);
        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(direct.LightOf(0), stack.LightOf(0));
            direct.Tick(); stack.Tick();
            Assert.Equal(direct.LightOf(0), stack.LightOf(0));
            Assert.Equal(direct.Acs.RunningCount, stack.Acs.RunningCount);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void UnderflowStopsBeforeActionOrFollowingMutation(int count)
    {
        var sim = Room(); var words = new List<int>();
        foreach (var arg in new[] { 7, 35, 0, 0 }.Take(count - 1)) words.AddRange([3, arg]);
        words.AddRange([3 + count, 112, 10, 112, 7, 5, 1]);
        Run(sim, words.ToArray());
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void CallsConsumeOnlyTheirOwnArguments()
    {
        var sim = Room();
        // Preserve a tag below the two arguments; the following one-argument call consumes it.
        Run(sim, 3, 7, 3, 7, 3, 35, 5, 112, 4, 112, 1);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void ArithmeticResultBecomesSignedLightArgument()
    {
        var sim = Room(); Run(sim, 3, 7, 3, 200, 3, 100, 14, 5, 110, 1);
        Assert.Equal(428, sim.LightOf(0));
    }

    [Fact]
    public void MissingSpecialOperandDoesNotExecute()
    {
        var sim = Room(); Run(sim, 3, 7, 3, 35, 5);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void UnsupportedStackSpecialStopsBeforeFollowingMutation()
    {
        var sim = Room(); Run(sim, 3, 7, 4, 999, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
