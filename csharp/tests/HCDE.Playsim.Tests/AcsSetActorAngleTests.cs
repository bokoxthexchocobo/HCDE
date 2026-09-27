using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetActorAngleTests
{
    [Theory]
    [InlineData(0, 0u)]
    [InlineData(16384, 0x40000000u)]
    [InlineData(-16384, 0xc0000000u)]
    [InlineData(65536, 0u)]
    [InlineData(81920, 0x40000000u)]
    [InlineData(int.MaxValue, 0xffff0000u)]
    public void ZeroTidChangesOnlyActivatorAndWrapsAcsTurns(int angle, uint expected)
    {
        var sim = Room(); Run(sim, sim.Actors[0], [3, 0, 3, angle, 276, 1]);
        Assert.Equal(expected, sim.Actors[0].Angle.Raw); Assert.Equal(0u, sim.Actors[1].Angle.Raw);
    }

    [Fact]
    public void NonzeroTidChangesAllMatchesIncludingDeadButNotDestroyedActors()
    {
        var sim = Room(); sim.Actors[0].Health = 0;
        Run(sim, null, [3, 42, 3, 16384, 276, 1]);
        Assert.All(sim.Actors, actor => Assert.Equal(BamAngle.Angle90, actor.Angle.Raw));
        sim.Actors[1].Destroy();
        Run(sim, null, [3, 42, 3, 32768, 276, 1]);
        Assert.Equal(BamAngle.Angle180, sim.Actors[0].Angle.Raw);
        Assert.Equal(BamAngle.Angle90, sim.Actors[1].Angle.Raw);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void MissingTargetStillContinuesScript(int tid)
    {
        var sim = Room(); Run(sim, null, [3, tid, 3, 16384, 276, 10, 112, 7, 35, 1]);
        Assert.All(sim.Actors, actor => Assert.Equal(0u, actor.Angle.Raw)); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingArgumentsStopWithoutMutation(bool oneArgument)
    {
        var sim = Room(); Run(sim, sim.Actors[0], oneArgument
            ? [3, 42, 276, 10, 112, 7, 35, 1] : [276, 10, 112, 7, 35, 1]);
        Assert.Equal(128, sim.LightOf(0)); Assert.All(sim.Actors, actor => Assert.Equal(0u, actor.Angle.Raw));
    }

    [Fact]
    public void SetterPreservesLowerStackAndQuerySeesNewAngle()
    {
        var sim = Room(); Run(sim, sim.Actors[0], [3, 7, 3, 0, 3, 16384, 276, 3, 0, 260, 3, 16384, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, Actor? actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], actor)); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Id = 42 }, new LevelThing { Type = 2, Id = 42 }],
    });
}
