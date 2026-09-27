using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsInvasionFunctionTests
{
    [Fact]
    public void QueriesFollowCommittedCountdownKillsAndNextWave()
    {
        var sim = Room(); sim.Invasion.Configure(2, 2, 2); sim.Invasion.Enabled = true;
        AssertQueries(sim, [1, 0, 0, 2, 0, 0, 0, 0]);
        sim.Invasion.Tick(sim); AssertQueries(sim, [2, 2, 0, 2, 0, 0, 0, 0]);
        sim.Invasion.Tick(sim); AssertQueries(sim, [2, 1, 0, 2, 0, 0, 0, 0]);
        sim.Invasion.Tick(sim); AssertQueries(sim, [4, 0, 1, 2, 1, 1, 0, 1]);
        Assert.Single(sim.Actors.OfType<BotPawn>()).Health = 0;
        sim.Invasion.Tick(sim); AssertQueries(sim, [5, 2, 1, 2, 1, 1, 1, 0]);
        sim.Invasion.Tick(sim); AssertQueries(sim, [5, 1, 1, 2, 1, 1, 1, 0]);
        sim.Invasion.Tick(sim); AssertQueries(sim, [4, 0, 2, 2, 1, 1, 0, 1]);
        sim.Actors.OfType<BotPawn>().Single(b => !b.IsDead).Health = 0;
        sim.Invasion.Tick(sim); AssertQueries(sim, [6, 0, 2, 2, 1, 1, 1, 0]);
    }

    [Fact]
    public void NativeAndClassicStateQueriesUseDifferentIds()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Tick(sim);
        Run(sim, 3, 7, 351, 0, 19700, 5, 112, 1); Assert.Equal(4, sim.LightOf(0));
        Run(sim, 3, 7, 130, 5, 112, 1); Assert.Equal(6, sim.LightOf(0));
        sim.Invasion.Enabled = false;
        Run(sim, 3, 7, 351, 0, 19700, 5, 112, 1); Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1, 19700)]
    [InlineData(1, 19700)]
    [InlineData(int.MaxValue, 19700)]
    [InlineData(0, 999)]
    public void UnsupportedCallStopsBeforeFollowingMutation(int count, int function)
    {
        var sim = Room(); Run(sim, 351, count, function, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void MissingFunctionOperandStopsWithoutPushingAResult()
    {
        var sim = Room(); Run(sim, 351, 0);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void AssertQueries(AuthoritySimulation sim, int[] expected)
    {
        for (var i = 0; i < expected.Length; i++)
        {
            Run(sim, 3, 7, 351, 0, 19700 + i, 5, 112, 1);
            Assert.Equal(expected[i], sim.LightOf(0));
        }
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
