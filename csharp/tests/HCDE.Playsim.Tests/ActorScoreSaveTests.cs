using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorScoreSaveTests
{
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-7)]
    [InlineData(0)]
    [InlineData(123)]
    [InlineData(int.MaxValue)]
    public void MapScoreAndSignedRangeRoundTrip(int score)
    {
        var sim = Room(score); var actor = sim.Actors.Single(); Assert.Equal(score, actor.Score);
        var saved = SimSavegame.Write(sim, true); actor.Score = 42; SimSavegame.Apply(sim, saved);
        Assert.Equal(score, actor.Score); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RuntimeScriptScoreSurvivesIdentityReconstruction()
    {
        var sim = Room(123); var actor = sim.Actors.Single();
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.Score, -7);
        Assert.Equal(-7, actor.Score);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(-7, sim.Actors.Single(a => !a.Destroyed).Score);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OlderSnapshotResetsLaterScore()
    {
        var sim = Room(0); var saved = SimSavegame.Write(sim); sim.Actors.Single().Score = 123;
        SimSavegame.Apply(sim, saved); Assert.Equal(0, sim.Actors.Single().Score);
    }

    [Fact]
    public void PlayerScoreDoesNotReplaceFragStatistics()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, Score = 123 }] });
        var player = sim.Players.Single(); player.FragCount = 9; var saved = SimSavegame.Write(sim);
        player.Score = -7; SimSavegame.Apply(sim, saved); Assert.Equal(123, player.Score); Assert.Equal(9, player.FragCount);
    }

    [Fact]
    public void ParsedMapScoreReachesDefaultStartup()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 3004; score = -7; skill3 = true; single = true; }", out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01")); Assert.Equal(-7, sim.Actors.Single().Score);
    }

    [Theory]
    [InlineData(0, 237)]
    [InlineData(4, int.MaxValue)]
    public void MalformedScoreArchiveRejectsBeforeMutation(int offset, int value)
    {
        var sim = Room(123); var saved = SimSavegame.Write(sim); var checksum = sim.Checksum;
        var start = ArchiveLayerFixture.TrailerStart(saved, 237);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-actorscore-header", error);
        Assert.Equal(checksum, sim.Checksum);
    }

    private static AuthoritySimulation Room(int score) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Score = score }]
    });
}
