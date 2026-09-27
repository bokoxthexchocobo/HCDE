using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsExecuteAlwaysTests
{
    [Fact]
    public void ConcurrentInstancesRetainArgumentsAcrossDelayAndDoNotBlockNormalExecute()
    {
        var sim = Room(); Add(sim, 1, 1, 56, 1, 3, 7, 28, 0, 5, 110, 1);
        Assert.True(sim.Acs.ExecuteAlways(1, [3])); Assert.True(sim.Acs.ExecuteAlways(1, [7]));
        Assert.True(sim.Acs.TryExecute(1, [11])); Assert.False(sim.Acs.TryExecute(1, [99]));
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(3, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(149, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NumberedControlAffectsOnlyNormalInstanceEvenWhenAlwaysWasStartedFirst(bool terminate)
    {
        var sim = Room(); Add(sim, 1, 1, 56, 1, 3, 7, 28, 0, 5, 110, 1);
        Assert.True(sim.Acs.ExecuteAlways(1, [3])); Assert.True(sim.Acs.TryExecute(1, [11]));
        sim.Acs.Tick(sim);
        Assert.True(terminate ? sim.Acs.TerminateScript(1) : sim.Acs.SuspendScript(1));
        sim.Acs.Tick(sim); Assert.Equal(131, sim.LightOf(0));
        Assert.Equal(terminate ? 0 : 1, sim.Acs.SuspendedCount);
        if (!terminate)
        {
            Assert.True(sim.Acs.ExecuteAlways(1, [7]));
            Assert.True(sim.Acs.TryExecute(1, [99])); sim.Acs.Tick(sim);
            Assert.Equal(142, sim.LightOf(0)); sim.Acs.Tick(sim); Assert.Equal(149, sim.LightOf(0));
        }
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void SelfSuspendedAlwaysInstanceCannotBeResumedOrTerminatedByNumber()
    {
        var sim = Room(); Add(sim, 1, 0, 2, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.ExecuteAlways(1, [])); sim.Acs.Tick(sim);
        var checksum = sim.Acs.Checksum;
        Assert.True(sim.Acs.SuspendScript(1)); Assert.True(sim.Acs.TerminateScript(1));
        Assert.Equal(checksum, sim.Acs.Checksum);
        Assert.False(sim.Acs.TryLoadMapArrays(new([], []), out _));
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(2, sim.Acs.SuspendedCount); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(1, sim.Acs.SuspendedCount);
    }

    [Fact]
    public void ControlModeParticipatesInChecksumAndMissingProgramDoesNotMutateState()
    {
        var normal = Room(); var always = Room(); Add(normal, 1, 0, 1); Add(always, 1, 0, 1);
        Assert.True(normal.Acs.TryExecute(1, [])); Assert.True(always.Acs.ExecuteAlways(1, []));
        Assert.NotEqual(normal.Acs.Checksum, always.Acs.Checksum);
        var checksum = always.Acs.Checksum;
        Assert.False(always.Acs.ExecuteAlways(999, [])); Assert.Equal(checksum, always.Acs.Checksum);
        Assert.False(always.Acs.TryLoadMapArrays(new([], []), out _));
        always.Acs.Tick(always); Assert.True(always.Acs.TryLoadMapArrays(new([], []), out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapLineForwardsAllArgumentsAndHonorsRepeat(bool repeat)
    {
        var sim = Room(); Add(sim, 1, 3, 3, 7, 28, 0, 28, 1, 14, 28, 2, 14, 5, 110, 1);
        var line = new LevelLine { Special = 226, Arg0 = 1, Arg2 = 2, Arg3 = 3, Arg4 = 5, PlayerUse = true, Repeat = repeat };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(repeat ? 226 : 0, line.Special);
        if (repeat) Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        sim.Acs.Tick(sim); Assert.Equal(repeat ? 148 : 138, sim.LightOf(0));
    }

    [Theory]
    [InlineData(999, 0)]
    [InlineData(1, 2)]
    public void FailedMapActivationPreservesLineAndVm(int script, int map)
    {
        var sim = Room(); Add(sim, 1, 0, 1); var checksum = sim.Acs.Checksum;
        var line = new LevelLine { Special = 226, Arg0 = script, Arg1 = map, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(226, line.Special); Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Theory]
    [InlineData(263, 2, 1)]
    [InlineData(382, 2, 1)]
    [InlineData(263, 999, 0)]
    [InlineData(382, 999, 0)]
    public void ScriptResultDispatchReportsSuccessAndAllowsConcurrentChild(int opcode, int child, int expected)
    {
        var sim = Room(); Add(sim, 2, 1, 3, 7, 28, 0, 5, 110, 1);
        Add(sim, 1, 0, 3, 7, 3, child, 3, 0, 3, 9, 3, 0, 3, 0, opcode, 226, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.ExecuteAlways(2, [4]));
        sim.Acs.Tick(sim); Assert.Equal(expected + 4, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(expected + 4 + (expected == 1 ? 9 : 0), sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void DirectDispatchStartsIndependentInstancesWithArguments()
    {
        var sim = Room(); Add(sim, 2, 1, 3, 7, 28, 0, 5, 110, 1);
        Add(sim, 1, 0, 11, 226, 2, 0, 3, 11, 226, 2, 0, 7, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(2, sim.Acs.RunningCount); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(138, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, int number, int argumentCount, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, ArgumentCount = argumentCount, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
