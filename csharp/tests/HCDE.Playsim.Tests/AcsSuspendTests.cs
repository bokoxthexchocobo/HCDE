using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSuspendTests
{
    [Theory]
    [InlineData(81, false)]
    [InlineData(81, true)]
    [InlineData(82, false)]
    [InlineData(82, true)]
    public void CurrentMapControlSucceedsWithoutMatchingScriptAndHonorsRepeat(int special, bool repeat)
    {
        var sim = Room(); var checksum = sim.Acs.Checksum;
        var line = new LevelLine { Special = special, Arg0 = 999, PlayerUse = true, Repeat = repeat };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(repeat ? special : 0, line.Special); Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Theory]
    [InlineData(81)]
    [InlineData(82)]
    public void UnsupportedMapControlDoesNotChangeScriptOrConsumeLine(int special)
    {
        var sim = Room(); Add(sim, 56, 5, 1); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var checksum = sim.Acs.Checksum;
        var line = new LevelLine { Special = special, Arg0 = 1, Arg1 = 2, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(special, line.Special); Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Fact]
    public void ExternalSuspendFreezesScriptAndResumeDiscardsPriorDelay()
    {
        var sim = Room(); Add(sim, 56, 100, 3, 7, 28, 0, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [19])); sim.Acs.Tick(sim);
        var line = new LevelLine { Special = 81, Arg0 = 1, PlayerCross = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.Equal(1, sim.Acs.SuspendedCount); var checksum = sim.Acs.Checksum;
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(checksum, sim.Acs.Checksum);
        Assert.True(sim.Acs.TryExecute(1, [99])); sim.Acs.Tick(sim);
        Assert.Equal(19, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TerminateStopsDelayedOrSuspendedScriptAndAllowsFreshExecution(bool suspended)
    {
        var sim = Room(); Add(sim, 56, 2, 3, 7, 28, 0, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [19])); sim.Acs.Tick(sim);
        if (suspended) Assert.True(sim.Acs.SuspendScript(1));
        var line = new LevelLine { Special = 82, Arg0 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(0, sim.Acs.SuspendedCount);
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TryLoadMapArrays(new([], []), out _));
        Assert.True(sim.Acs.TryExecute(1, [35])); sim.Acs.Tick(sim); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SuspendBeforeFirstTickDoesNotLoseEntryPointOrArguments()
    {
        var sim = Room(); Add(sim, 3, 7, 28, 0, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [19])); Assert.True(sim.Acs.SuspendScript(1));
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TryExecute(1, [35])); sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void SuspendRetainsLocalsAndArraysAndResumeIgnoresNewArguments()
    {
        var sim = Room(); Add(sim, 3, 0, 3, 23, 364, 0, 2, 3, 7, 28, 0, 3, 0, 365, 0, 14, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [19])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.Acs.RunningCount); Assert.Equal(1, sim.Acs.SuspendedCount); Assert.Equal(128, sim.LightOf(0));
        var checksum = sim.Acs.Checksum;
        for (var i = 0; i < 5; i++) sim.Acs.Tick(sim);
        Assert.Equal(checksum, sim.Acs.Checksum);
        Assert.True(sim.Acs.TryExecute(1, [99])); Assert.Equal(0, sim.Acs.SuspendedCount);
        Assert.NotEqual(checksum, sim.Acs.Checksum);
        Assert.False(sim.Acs.TryExecute(1, [35])); sim.Acs.Tick(sim);
        Assert.Equal(42, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ResumeContinuesAfterEachSuspendRatherThanRestarting()
    {
        var sim = Room(); Add(sim, 46, 0, 2, 46, 0, 2, 3, 7, 28, 0, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(1, sim.Acs.SuspendedCount);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(2, sim.LightOf(0));
    }

    [Fact]
    public void ResumeUsesOriginalDefinitionAfterProgramReplacement()
    {
        var sim = Room(); Add(sim, 2, 10, 112, 7, 19, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Add(sim, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SuspendedScriptsPreventMapStorageReload()
    {
        var sim = Room(); Add(sim, 2, 1); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var checksum = sim.Acs.Checksum;
        Assert.False(sim.Acs.TryLoadMapArrays(new([], []), out _)); Assert.Equal(checksum, sim.Acs.Checksum);
    }

    [Fact]
    public void MapLineResumesWithoutReplacingArgumentsAndConsumesSuccessfulOneShot()
    {
        var sim = Room(); Add(sim, 2, 3, 7, 28, 0, 5, 112, 1);
        var first = new LevelLine { Special = 80, Arg0 = 1, Arg2 = 19, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), first, true)); sim.Acs.Tick(sim);
        var second = new LevelLine { Special = 80, Arg0 = 1, Arg2 = 99, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), second, true)); Assert.Equal(0, second.Special);
        sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void DelayAfterResumeUsesNormalTickTiming()
    {
        var sim = Room(); Add(sim, 2, 56, 2, 10, 112, 7, 19, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
    }

    [Fact]
    public void OtherScriptsContinueWhileOneIsSuspended()
    {
        var sim = Room(); Add(sim, 2, 1); Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Add(new AcsProgram { Number = 2, Code = Code(10, 112, 7, 35, 1) }); Assert.True(sim.Acs.Enqueue(2));
        sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(1, sim.Acs.SuspendedCount);
    }

    private static void Add(AuthoritySimulation sim, params int[] words) => sim.Acs.Add(new AcsProgram
    { Number = 1, Code = Code(words), ArgumentCount = 1, LocalArraySizes = [1] });
    private static byte[] Code(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }] });
}
