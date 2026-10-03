using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScoreTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(123456)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void SetGetCheckPreserveExactIntegerScore(int value)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 22, 3, value, 245,
            3, 7, 3, 0, 3, 22, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.Score); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 22, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 22, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void NonPlayerAndDeadActorsRetainScriptScore()
    {
        var sim = Room(); var actor = sim.AddBot(128, 0); actor.Health = 0;
        Run(sim, actor, [3, 0, 3, 22, 3, -7, 245,
            3, 7, 3, 0, 3, 22, 246, 3, -7, 19, 5, 112, 1]);
        Assert.Equal(-7, actor.Score); Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetsIgnoreWritesAndDoNotMatchZero(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 22, 3, 123, 245,
            3, 7, 3, tid, 3, 22, 246, 5, 112, 1]);
        Assert.Equal(0, actor.Score); Assert.Equal(0, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, tid, 3, 22, 3, 0, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void TidWritesAllMatchesAndQueryUsesNewestActor()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3001, Id = 42 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
        });
        var actor = sim.Actors.First();
        Run(sim, actor, [3, 42, 3, 22, 3, 123, 245, 1]);
        Assert.All(sim.Actors, candidate => Assert.Equal(123, candidate.Score));
        sim.Actors.Last().Score = -99;
        Run(sim, actor, [3, 7, 3, 42, 3, 22, 246, 3, -99, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void ScoreParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().Score = 123;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void ActorScoreAndPlayerFragsAreIndependent()
    {
        var sim = Room(); var player = sim.Players.Single(); player.FragCount = 9;
        Run(sim, player, [3, 0, 3, 22, 3, 123, 245, 1]);
        Assert.Equal(123, player.Score); Assert.Equal(9, player.FragCount);
        player.FragCount = -2; Assert.Equal(123, player.Score);
    }

    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
