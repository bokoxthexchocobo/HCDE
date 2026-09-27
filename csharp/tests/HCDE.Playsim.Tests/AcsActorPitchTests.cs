using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorPitchTests
{
    [Theory]
    [InlineData(8192, 45)]
    [InlineData(-8192, -45)]
    [InlineData(16384, 90)]
    [InlineData(49152, -90)]
    [InlineData(65536, 0)]
    public void MonsterPitchNormalizesAcsTurnsAndRoundTrips(int value, int degrees)
    {
        var sim = Room(3001); var actor = sim.Actors.Single();
        Run(sim, [3, 0, 3, value, 332, 3, 7, 3, 0, 331, 3, degrees * 65536 / 360, 19, 5, 112, 1]);
        Assert.Equal(degrees, actor.PitchDegrees); Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(16384, 89)]
    [InlineData(-16384, -89)]
    public void PlayerPitchRetainsManagedViewLimits(int value, int degrees)
    {
        var sim = Room(1); Run(sim, [3, 0, 3, value, 332, 1]);
        Assert.Equal(degrees, sim.Players.Single().PitchDegrees);
    }

    [Fact]
    public void TidSetterChangesAllMatchingActorsAndLeavesYawAlone()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Id = 42 }, new LevelThing { Type = 2, Id = 42 }],
        });
        Run(sim, [3, 42, 3, 8192, 332, 1]);
        Assert.All(sim.Actors, actor => { Assert.Equal(45, actor.PitchDegrees); Assert.Equal(0u, actor.Angle.Raw); });
    }

    [Theory]
    [InlineData(331)]
    [InlineData(332)]
    public void MissingArgumentsStopFollowingAction(int opcode)
    {
        var sim = Room(1); Run(sim, [opcode, 10, 112, 7, 35, 1]);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Actors.Single().PitchDegrees);
    }

    [Fact]
    public void MissingActorPitchReturnsZero()
    {
        var sim = Room(1); Run(sim, [3, 7, 3, 999, 331, 5, 112, 1]); Assert.Equal(0, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Actors.FirstOrDefault())); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = [new LevelThing { Type = type }],
    });
}
