using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScriptWaitCompatibilityTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void OnlyLegacyDirectWaitContinuesWhenTargetNeverStarts(bool legacy, bool direct, bool completes)
    {
        var sim = Room(legacy); Waiter(sim, direct);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); // Even the legacy form yields before resuming.
        sim.Acs.Tick(sim); Assert.Equal(completes ? 35 : 128, sim.LightOf(0));
        Assert.Equal(completes ? 0 : 1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExistingDelayedOrSuspendedTargetStillBlocksLegacyDirectWait(bool suspended)
    {
        var sim = Room(true); Waiter(sim, true); Add(sim, 2, suspended ? [2, 1] : [56, 100, 1]);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.TryExecute(2, []));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TerminateScript(2)); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void TargetStartedBetweenWaitAndNextTickIsObserved()
    {
        var sim = Room(true); Waiter(sim, true); Add(sim, 2, 2, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TerminateScript(2)); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void AlwaysInstanceDoesNotBlockLegacyDirectWait()
    {
        var sim = Room(true); Waiter(sim, true); Add(sim, 2, 2, 1);
        Assert.True(sim.Acs.ExecuteAlways(2, [])); Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(1, sim.Acs.SuspendedCount);
    }

    [Fact]
    public void CompatibilityWaitStateChangesVmChecksumForOtherwiseIdenticalScripts()
    {
        var normal = Room(false); var legacy = Room(true);
        Waiter(normal, true); Waiter(legacy, true);
        Assert.True(normal.Acs.TryExecute(1, [])); Assert.True(legacy.Acs.TryExecute(1, []));
        Assert.Equal(normal.Acs.Checksum, legacy.Acs.Checksum);
        normal.Acs.Tick(normal); legacy.Acs.Tick(legacy);
        Assert.NotEqual(normal.Acs.Checksum, legacy.Acs.Checksum);
    }

    [Fact]
    public void LegacyDirectWaitRejectsTruncatedOperand()
    {
        var sim = Room(true); Add(sim, 1, 82);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(128, sim.LightOf(0));
    }

    private static void Waiter(AuthoritySimulation sim, bool direct) =>
        Add(sim, 1, (direct ? new[] { 82, 2 } : new[] { 3, 2, 81 }).Concat([10, 112, 7, 35, 1]).ToArray());

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
    }

    private static AuthoritySimulation Room(bool legacy) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }]
    }, compat: legacy ? CompatSurface.LegacyScriptWaitDirect : CompatSurface.None);
}
