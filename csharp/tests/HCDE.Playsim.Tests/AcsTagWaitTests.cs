using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTagWaitTests
{
    [Theory]
    [InlineData(false, 23)]
    [InlineData(true, 23)]
    [InlineData(false, 41)]
    [InlineData(true, 41)]
    public void WaitsForMovingPlaneToFinish(bool direct, int special)
    {
        var sim = Room(); Waiter(sim, direct); Activate(sim, special, 7, 8, 3);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingTagYieldsOnceWithoutWaitingForFutureMovement(bool direct)
    {
        var sim = Room(); Waiter(sim, direct, 999);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void WaitsForBothPlanesAndIgnoresUnrelatedSector()
    {
        var sim = Room(); Waiter(sim, true);
        Activate(sim, 23, 7, 8, 2); Activate(sim, 41, 7, 8, 4); Activate(sim, 41, 8, 8, 100);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(128, sim.LightOf(0)); sim.Tick(); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(132, sim.CeilingOf(1));
    }

    [Fact]
    public void PausedCeilingRetainsWaitUntilMoverIsRemoved()
    {
        var sim = Room(); Waiter(sim, true); Activate(sim, 41, 7, 8, 10);
        Activate(sim, 44, 7, 0, 0);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick(); sim.Tick();
        Assert.Equal(128, sim.LightOf(0)); Assert.False(sim.Acs.TryLoadMapArrays(new([], []), out _));
        Activate(sim, 276, 7, 0, 0); sim.Tick(); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SuspendResumeDiscardsTagWaitAndContinuesAfterInstruction()
    {
        var sim = Room(); Waiter(sim, true); Activate(sim, 41, 7, 8, 100);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(sim.Acs.SuspendScript(1)); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(128, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(61)]
    [InlineData(62)]
    public void MalformedWaitTerminates(int opcode)
    {
        var sim = Room(); Add(sim, opcode); Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void SavedTagParticipatesInChecksumWithSameCodeAndLocals()
    {
        var a = Room(); var b = Room();
        Add(a, 28, 0, 3, 0, 25, 0, 61, 1); Add(b, 28, 0, 3, 0, 25, 0, 61, 1);
        Assert.True(a.Acs.TryExecute(1, [7])); Assert.True(b.Acs.TryExecute(1, [8]));
        a.Acs.Tick(a); b.Acs.Tick(b); Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    private static void Activate(AuthoritySimulation sim, int special, int tag, int speed, int amount) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = special, Arg0 = tag, Arg1 = speed, Arg2 = amount, PlayerUse = true }, true));

    private static void Waiter(AuthoritySimulation sim, bool direct, int tag = 7) =>
        Add(sim, (direct ? new[] { 62, tag } : new[] { 3, tag, 61 }).Concat([10, 112, 7, 35, 1]).ToArray());

    private static void Add(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, ArgumentCount = 1, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
