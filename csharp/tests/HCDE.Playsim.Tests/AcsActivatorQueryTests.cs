using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActivatorQueryTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 3)]
    [InlineData(3001, -1)]
    [InlineData(0, -1)]
    public void PlayerNumberReportsVisiblePlayerSlotOrMinusOne(int type, int expected)
    {
        var sim = Room(type, 42); Query(sim, 247, sim.Actors.FirstOrDefault());
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(1, 42)]
    [InlineData(3001, 73)]
    [InlineData(1, 0)]
    [InlineData(0, 0)]
    public void ActivatorTidUsesMapThingId(int type, int tid)
    {
        var sim = Room(type, tid); Query(sim, 248, sim.Actors.FirstOrDefault());
        Assert.Equal(tid, sim.LightOf(0));
    }

    [Theory]
    [InlineData(247, -1)]
    [InlineData(248, 0)]
    public void DestroyedActivatorUsesWorldDefault(int opcode, int expected)
    {
        var sim = Room(1, 42); var actor = sim.Actors.Single(); actor.Destroy();
        Query(sim, opcode, actor); Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(247, 0)]
    [InlineData(248, 42)]
    public void DeadButExistingActivatorRetainsIdentity(int opcode, int expected)
    {
        var sim = Room(1, 42); var actor = sim.Actors.Single(); actor.Health = 0;
        Query(sim, opcode, actor); Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void ThingIdentityParticipatesInSimulationChecksum()
    {
        Assert.NotEqual(Room(1, 42).Checksum, Room(1, 43).Checksum);
    }

    private static void Query(AuthoritySimulation sim, int opcode, Actor? actor)
    {
        int[] words = [3, 7, opcode, 5, 112, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], actor)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room(int type, int tid) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = type == 0 ? [] : [new LevelThing { Type = type, Id = tid }],
    });
}
