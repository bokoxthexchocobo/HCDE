using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPitchQueryTests
{
    [Theory]
    [InlineData(450, 16384)]
    [InlineData(-450, -16384)]
    [InlineData(180, -32768)]
    [InlineData(-180, -32768)]
    [InlineData(540, -32768)]
    [InlineData(360, 0)]
    [InlineData(-720, 0)]
    [InlineData(32767, 1274)]
    [InlineData(-32768, -1456)]
    [InlineData(-1, -182)]
    [InlineData(0.5, 91)]
    [InlineData(-0.5, -91)]
    public void QueryNormalizesPitchAndTruncatesSignedFixedTurns(double pitch, int expected)
    {
        var sim = Room();
        var actor = sim.Actors.Single(); actor.PitchDegrees = pitch;
        AssertQuery(sim, actor, 7, expected);
        Assert.Equal(pitch, actor.PitchDegrees);
    }

    [Fact]
    public void ZeroTidUsesActivator()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.PitchDegrees = 450;
        AssertQuery(sim, actor, 0, 16384);
    }

    [Fact]
    public void MissingTidReturnsZero()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.PitchDegrees = 450;
        AssertQuery(sim, actor, 99, 0);
    }

    [Fact]
    public void ImportedPitchIsNormalizedAtQueryBoundary()
    {
        const string text = "namespace = \"ZDoom\"; sector { heightceiling = 128; id = 7; lightlevel = 128; } "
            + "thing { type = 2035; id = 7; pitch = 450; skill3 = true; coop = true; single = true; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        AssertQuery(sim, sim.Actors.Single(), 7, 16384);
        Assert.Equal(450, sim.Actors.Single().PitchDegrees);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 2035, Id = 7 }],
    });

    private static void AssertQuery(AuthoritySimulation sim, Actor activator, int tid, int expected)
    {
        int[] words = [(int)AcsPcode.PushNumber, 7, (int)AcsPcode.PushNumber, tid,
            (int)AcsPcode.GetActorPitch, (int)AcsPcode.PushNumber, expected,
            (int)AcsPcode.Eq, (int)AcsPcode.Lspec2, 112, (int)AcsPcode.Terminate];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(1, sim.LightOf(0));
    }
}
