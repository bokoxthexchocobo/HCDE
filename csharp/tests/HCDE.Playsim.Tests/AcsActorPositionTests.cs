using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorPositionTests
{
    [Theory]
    [InlineData(196, 81920)]
    [InlineData(197, -163840)]
    [InlineData(198, 245760)]
    public void ActivatorCoordinatesPreserveFixedPointFractions(int opcode, int expected)
    {
        var sim = Room(); var actor = sim.Actors[0];
        actor.X = Fixed.FromDouble(1.25); actor.Y = Fixed.FromDouble(-2.5); actor.Z = Fixed.FromDouble(3.75);
        Run(sim, actor, [3, 7, 3, 0, opcode, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TidLookupUsesNewestRemainingActor(bool destroyNewest)
    {
        var sim = Room(); sim.Actors[0].X = Fixed.FromInt(10); sim.Actors[1].X = Fixed.FromInt(20);
        if (destroyNewest) sim.Actors[1].Destroy();
        Run(sim, null, [3, 7, 3, 42, 196, 3, (destroyNewest ? 10 : 20) * 65536, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void MissingActorReturnsZero(int tid)
    {
        var sim = Room(); Run(sim, null, [3, 7, 3, tid, 196, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(196)]
    [InlineData(197)]
    [InlineData(198)]
    [InlineData(259)]
    [InlineData(260)]
    [InlineData(282)]
    public void MissingArgumentStopsBeforeFollowingAction(int opcode)
    {
        var sim = Room(); Run(sim, null, [opcode, 10, 112, 7, 35, 1]);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Run(AuthoritySimulation sim, Actor? activator, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], activator)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1, Id = 42 }, new LevelThing { Type = 2, Id = 42 }],
    });
}
