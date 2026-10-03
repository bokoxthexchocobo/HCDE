using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsReactionTimeTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(-7, false)]
    [InlineData(int.MaxValue, false)]
    [InlineData(0, true)]
    [InlineData(-7, true)]
    [InlineData(123, true)]
    public void SetGetCheckPreserveIntegerCounter(int value, bool monster)
    {
        var sim = Room(); var actor = monster ? Monster(sim) : (Actor)sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 37, 3, value, 245,
            3, 7, 3, 0, 3, 37, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.ReactionTime); Assert.Equal(1, sim.LightOf(0));
        if (monster) Assert.Equal(value, actor.Brain!.ReactionTics);
        Run(sim, actor, [3, 7, 3, 0, 3, 37, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 37, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void CounterDelaysAttackAndQueryTracksCountdown()
    {
        var sim = Room(); var actor = Monster(sim); actor.ChaseSpeed = 0;
        Set(sim, actor, 3);
        for (var expected = 2; expected >= 0; expected--)
        {
            sim.Tick(); Assert.Equal(expected, actor.ReactionTime);
            Assert.NotEqual(MonsterMode.Windup, actor.Brain!.Mode);
            Run(sim, actor, [3, 7, 3, 0, 3, 37, 246, 3, expected, 19, 5, 112, 1]);
            Assert.Equal(1, sim.LightOf(0));
        }
        sim.Tick(); Assert.Equal(MonsterMode.Windup, actor.Brain!.Mode);
    }

    [Fact]
    public void DamageWakeUpClearsTheExposedCounter()
    {
        var sim = Room(); var actor = Monster(sim); actor.PainChance = 0; Set(sim, actor, 123);
        ActorDamage.Apply(actor, 1, sim.Players.Single());
        Assert.Equal(0, actor.ReactionTime); Assert.Equal(0, actor.Brain!.ReactionTics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetIgnoresSetter(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 37, 3, 123, 245, 3, 7, 3, tid, 3, 37, 246, 5, 112, 1]);
        Assert.Equal(0, actor.ReactionTime); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void NonBrainCounterParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().ReactionTime = 123;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.DoomEdNum == 3002);
    private static void Set(AuthoritySimulation sim, Actor actor, int value) =>
        Run(sim, actor, [3, 0, 3, 37, 3, value, 245, 1]);
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 3002 }, new LevelThing { Type = 1, X = 50 }],
    });
}
