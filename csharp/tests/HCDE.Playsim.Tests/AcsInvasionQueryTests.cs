using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsInvasionQueryTests
{
    [Fact]
    public void OpcodeIdsMatchNativeBytecodeRatherThanManagedEnumAssumptions()
    {
        Assert.Equal(129, (int)AcsPcode.Lspec6);
        Assert.Equal(130, (int)AcsPcode.Lspec6Direct);
    }

    [Fact]
    public void QueriesTrackCountdownWaveAndIntermissionWithoutAdvancingDirector()
    {
        var sim = Room(); sim.Invasion.Configure(0, 2, 2); sim.Invasion.Enabled = true;
        AssertQueries(sim, 0, 0);
        sim.Invasion.Tick(sim); AssertQueries(sim, 0, 5);
        Assert.Equal(2, sim.Invasion.Cooldown);
        sim.Invasion.Tick(sim); AssertQueries(sim, 0, 5);
        sim.Invasion.Tick(sim); AssertQueries(sim, 1, 6);
        Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.Single(sim.Actors.OfType<BotPawn>()).Health = 0;
        sim.Invasion.Tick(sim); AssertQueries(sim, 1, 7);
        Assert.Equal(0, sim.Invasion.ActiveMonsters); Assert.Equal(1, sim.Invasion.Cleared);
        sim.Invasion.Tick(sim); AssertQueries(sim, 1, 7);
        sim.Invasion.Tick(sim); AssertQueries(sim, 2, 6);
    }

    [Fact]
    public void FinalWaveReportsClassicVictoryEight()
    {
        var sim = Room(); sim.Invasion.Configure(1, 0, 2); sim.Invasion.Enabled = true;
        sim.Invasion.Tick(sim); AssertQueries(sim, 1, 6);
        Assert.Single(sim.Actors.OfType<BotPawn>()).Health = 0;
        sim.Invasion.Tick(sim); AssertQueries(sim, 1, 8);
    }

    [Fact]
    public void DisabledStateReturnsZeroWithoutResettingWaveHistory()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Tick(sim);
        sim.Invasion.Enabled = false; AssertQueries(sim, 1, 0);
    }

    [Fact]
    public void QueriesTakeNoOperandsAndLeaveEarlierStackValuesIntact()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Tick(sim);
        Run(sim, 3, 7, 129, 130, 14, 5, 112, 1); // tag, wave+state, Light_ChangeToValue
        Assert.Equal(7, sim.LightOf(0));
    }

    [Fact]
    public void Opcode128IsNotMisinterpretedAsInvasionWave()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Tick(sim);
        Run(sim, 3, 7, 128, 5, 112, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    private static void AssertQueries(AuthoritySimulation sim, int wave, int state)
    {
        Run(sim, 3, 7, 129, 5, 112, 1); Assert.Equal(wave, sim.LightOf(0));
        Run(sim, 3, 7, 130, 5, 112, 1); Assert.Equal(state, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
