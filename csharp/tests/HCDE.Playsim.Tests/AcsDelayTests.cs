using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDelayTests
{
    [Theory]
    [InlineData(false, false, 1, 1)]
    [InlineData(true, false, 1, 1)]
    [InlineData(false, false, 3, 3)]
    [InlineData(true, false, 3, 3)]
    [InlineData(false, true, 1, 2)]
    [InlineData(true, true, 1, 2)]
    [InlineData(false, true, 0, 1)]
    [InlineData(true, true, 0, 1)]
    public void ResumesExactlyWhenCountdownReachesZero(bool stack, bool legacy, int delay, int wait)
    {
        var sim = Room();
        Start(sim, legacy, stack ? [3, delay, 55, 10, 112, 7, 35, 1] : [56, delay, 10, 112, 7, 35, 1]);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        for (var i = 1; i < wait; i++) { sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); }
        sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, -1)]
    [InlineData(true, -1)]
    public void NonpositiveDelayContinuesWithinSameTick(bool stack, int delay)
    {
        var sim = Room();
        Start(sim, false, stack ? [3, delay, 55, 10, 112, 7, 35, 1] : [56, delay, 10, 112, 7, 35, 1]);
        sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void StackDelayConsumesOnlyItsDurationAndPreservesTag()
    {
        var sim = Room(); Start(sim, false, 3, 7, 3, 1, 55, 3, 35, 5, 112, 1);
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void StackUnderflowStopsBeforeFollowingMutation()
    {
        var sim = Room(); Start(sim, false, 55, 10, 112, 7, 35, 1);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ZeroDelayLoopRemainsBoundedByInstructionBudget()
    {
        var sim = Room(); Start(sim, false, 56, 0, 52, 0);
        sim.Acs.Tick(sim); Assert.Equal(1, sim.Acs.RunningCount); Assert.Equal(128, sim.LightOf(0));
    }

    private static void Start(AuthoritySimulation sim, bool legacy, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, LegacyHexenDelay = legacy });
        Assert.True(sim.Acs.Enqueue(1));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
