using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScriptWaitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WaitsForExistingDelayedTargetBeforeContinuing(bool direct)
    {
        var sim = Room(); Waiter(sim, direct); Add(sim, 2, 56, 2, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.TryExecute(2, []));
        for (var i = 0; i < 3; i++) { sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); }
        sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingTargetMustStartAndFinishAndAlwaysInstancesDoNotCount(bool direct)
    {
        var sim = Room(); Waiter(sim, direct); Add(sim, 2, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var checksum = sim.Acs.Checksum;
        sim.Acs.Tick(sim); Assert.Equal(checksum, sim.Acs.Checksum);
        Assert.True(sim.Acs.ExecuteAlways(2, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void SuspendedTargetStillBlocksUntilTerminated()
    {
        var sim = Room(); Waiter(sim, true); Add(sim, 2, 2, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.TryExecute(2, []));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TerminateScript(2)); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SuspendAndResumeWaiterDiscardsPreviousWaitState(bool targetExists)
    {
        var sim = Room(); Waiter(sim, true); Add(sim, 2, 2, 1);
        Assert.True(sim.Acs.TryExecute(1, []));
        if (targetExists) Assert.True(sim.Acs.TryExecute(2, []));
        sim.Acs.Tick(sim); Assert.True(sim.Acs.SuspendScript(1)); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SelfWaitBlocksAndCanBeTerminatedWithoutFollowingMutation()
    {
        var sim = Room(); Add(sim, 1, 82, 1, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.False(sim.Acs.TryLoadMapArrays(new([], []), out _));
        Assert.True(sim.Acs.TerminateScript(1)); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(81)]
    [InlineData(82)]
    public void MissingOperandTerminatesInsteadOfWaiting(int opcode)
    {
        var sim = Room(); Add(sim, 1, opcode); Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(sim.Acs.TryLoadMapArrays(new([], []), out _));
    }

    [Fact]
    public void WaitTargetParticipatesInChecksumAfterStackOperandIsConsumed()
    {
        var a = Room(); var b = Room();
        // Clear the source local before waiting so only the saved target differs.
        Add(a, 1, 28, 0, 3, 0, 25, 0, 81, 1); Add(b, 1, 28, 0, 3, 0, 25, 0, 81, 1);
        Assert.True(a.Acs.TryExecute(1, [2])); Assert.True(b.Acs.TryExecute(1, [3]));
        a.Acs.Tick(a); b.Acs.Tick(b);
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    private static void Waiter(AuthoritySimulation sim, bool direct) =>
        Add(sim, 1, (direct ? new[] { 82, 2 } : new[] { 3, 2, 81 }).Concat([10, 112, 7, 35, 1]).ToArray());

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, ArgumentCount = 1, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }]
    });
}
