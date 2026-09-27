using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTimerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(35)]
    [InlineData(350)]
    public void AuthorityScriptsObserveStartOfTicTime(int elapsed)
    {
        var sim = Room();
        for (var i = 0; i < elapsed; i++) sim.Tick();
        Start(sim, Query(elapsed)); sim.Tick();
        Assert.Equal(1, sim.LightOf(0));
        Assert.Equal(elapsed + 1, sim.Thinkers.Clock.Tic);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(35)]
    [InlineData(int.MaxValue)]
    public void DirectVmTickReadsClockWithoutAdvancingIt(int elapsed)
    {
        var sim = Room(); sim.Thinkers.Clock.Restore(elapsed);
        Start(sim, Query(elapsed)); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0)); Assert.Equal(elapsed, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void DelayReportsElapsedLevelTics()
    {
        var sim = Room();
        // Keep the first Timer result across a delay, subtract it from the next result.
        Start(sim, [3, 7, 93, 56, 3, 93, 217, 15, 3, 3, 19, 5, 112, 1]);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void RepeatedQueriesWithinOneTickAreIdentical()
    {
        var sim = Room(); Start(sim, [3, 7, 93, 93, 19, 5, 112, 1]);
        sim.Tick(); Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void SuspendedScriptUsesResumeTime()
    {
        var sim = Room(); Start(sim, [2, .. Query(4)]);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Tick();
        Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void QueryUsesRestoredPoseClock()
    {
        var source = Room(); for (var i = 0; i < 7; i++) source.Tick();
        var sim = Room(); sim.RestoreState(source.CaptureState());
        Start(sim, Query(7)); sim.Tick();
        Assert.Equal(1, sim.LightOf(0)); Assert.Equal(8, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void NestedScriptReadsItsExecutionTick()
    {
        var sim = Room(); sim.Acs.Add(Program(2, Query(1)));
        Start(sim, [13, 226, 2, 0, 0, 0, 0, 1]);
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(1, sim.LightOf(0));
    }

    private static int[] Query(int expected) => [3, 7, 93, 3, expected, 19, 5, 112, 1];
    private static void Start(AuthoritySimulation sim, int[] words)
    {
        sim.Acs.Add(Program(1, words)); Assert.True(sim.Acs.TryExecute(1, []));
    }
    private static AcsProgram Program(int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
