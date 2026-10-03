using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileTests
{
    [Fact]
    public void AttackWaitsSixtySixTicsThenDamagesAndLaunchesTarget()
    {
        var (sim, vile) = Setup();
        Windup(sim, vile);
        var player = sim.Players.Single(); player.Health = 1000;
        for (var i = 0; i < 65; i++) sim.Tick();
        Assert.Equal(1000, player.Health);
        sim.Tick();
        Assert.Equal(918, player.Health); // 20 direct + (70 - (24 - player radius 16)).
        Assert.Equal(10, player.VelocityZ.ToDouble());
        Assert.Equal(vile.Id, player.LastDamageSourceId);
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
    }

    [Fact]
    public void BreakingSightBeforeBlastPreventsDamageAndThrust()
    {
        var (sim, vile) = Setup(); Windup(sim, vile);
        for (var i = 0; i < 65; i++) sim.Tick();
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine { X1 = 200, X2 = 200, Y1 = -200, Y2 = 200, SideBack = -1 });
        sim.Tick();
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.Equal(default, sim.Players.Single().VelocityZ);
    }

    [Theory]
    [InlineData(3001, 60, 34)]
    [InlineData(3004, 20, 20)]
    [InlineData(9, 30, 25)]
    [InlineData(66, 300, 30)]
    public void RaisesCorpseAtFullHealthAndWaitsThroughRaiseFrames(int type, int health, int duration)
    {
        var (sim, vile) = Setup(type);
        var corpse = sim.Actors.Single(actor => actor.DoomEdNum == type);
        corpse.Health = 0;
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(health, corpse.Health);
        Assert.True(corpse.BlocksActors);
        Assert.Equal(MonsterMode.Heal, vile.Brain!.Mode);
        Assert.Equal(MonsterMode.Raise, corpse.Brain!.Mode);
        Assert.InRange(corpse.Brain.RaiseTics, duration - 1, duration);
        Assert.Null(corpse.LastDamageSourceId);
        Assert.Null(corpse.Brain.TargetId);
        Assert.Equal(1, corpse.DeathCount);
        Assert.Equal(3, sim.Actors.Count);
    }

    [Theory]
    [InlineData(3006)]
    [InlineData(16)]
    [InlineData(7)]
    [InlineData(64)]
    public void ActorsWithoutRaiseStatesRemainDead(int type)
    {
        var (sim, _) = Setup(type);
        var corpse = sim.Actors.Last(actor => actor.DoomEdNum == type);
        corpse.Health = 0;
        for (var i = 0; i < 15; i++) sim.Tick();
        Assert.True(corpse.IsDead);
    }

    [Fact]
    public void CorpseOccupiedByAnotherActorCannotBeRaised()
    {
        var (sim, _) = Setup(3001);
        var corpse = sim.Actors.Single(actor => actor.DoomEdNum == 3001); corpse.Health = 0;
        var blocker = sim.AddBot(50, 0); blocker.Brain = null;
        for (var i = 0; i < 15; i++) sim.Tick();
        Assert.True(corpse.IsDead);
    }

    [Fact]
    public void HealingCanBeInterruptedByPain()
    {
        var (sim, vile) = Setup(3001);
        sim.Actors.Single(actor => actor.DoomEdNum == 3001).Health = 0;
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(MonsterMode.Heal, vile.Brain!.Mode);
        vile.PainChance = 256; ActorDamage.Apply(vile, 1); sim.Tick();
        Assert.Equal(MonsterMode.Pain, vile.Brain.Mode);
    }

    [Fact]
    public void ResurrectionRequiresEnoughCeilingClearance()
    {
        var (sim, _) = Setup(3003, 60);
        var corpse = sim.Actors.Single(actor => actor.DoomEdNum == 3003); corpse.Health = 0;
        for (var i = 0; i < 15; i++) sim.Tick();
        Assert.True(corpse.IsDead);
        Assert.Equal(64, corpse.Height.ToDouble());
    }

    [Theory]
    [InlineData(100, 10)]
    [InlineData(500, 2)]
    [InlineData(1000, 1)]
    public void AttackThrustUsesTargetMass(int mass, int speed)
    {
        var (sim, vile) = Setup();
        var player = sim.Players.Single(); player.Health = 1000; player.Mass = mass;
        Windup(sim, vile);
        for (var i = 0; i < 66; i++) sim.Tick();
        Assert.Equal(speed, player.VelocityZ.ToDouble());
    }

    [Fact]
    public void ResurrectionAndAttackReplayDeterministically()
    {
        var (left, _) = Setup(3001); var (right, _) = Setup(3001);
        foreach (var sim in new[] { left, right })
        {
            sim.Players.Single().Health = 1000;
            sim.Actors.Single(actor => actor.DoomEdNum == 3001).Health = 0;
        }
        for (var i = 0; i < 150; i++)
        {
            left.Tick(); right.Tick();
            Assert.Equal(left.Checksum, right.Checksum);
            Assert.Equal(left.CombatRandomState, right.CombatRandomState);
        }
    }

    [Fact]
    public void RaisedWaveMemberReturnsToLiveCountWithoutIncreasingSpawned()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 64 }, new LevelThing { Type = 1, X = 800 },
                new LevelThing { Type = 9001, X = 50 }, new LevelThing { Type = 9001, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        sim.Invasion.Enabled = true; sim.Invasion.Configure(1, 0, 35); sim.Tick();
        var corpse = sim.Actors.OfType<BotPawn>().First();
        corpse.Health = 0;
        sim.Tick();
        Assert.Equal(1, sim.Invasion.ActiveMonsters);
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(20, corpse.Health);
        Assert.Equal(MonsterMode.Raise, corpse.Brain!.Mode);
        Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase);
        Assert.Equal(2, sim.Invasion.ActiveMonsters);
        Assert.Equal(2, sim.Invasion.Spawned);
        Assert.Equal(0, sim.Invasion.Cleared);
    }

    private static void Windup(AuthoritySimulation sim, Actor vile)
    {
        for (var i = 0; i < 20 && vile.Brain!.Mode != MonsterMode.Windup; i++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, vile.Brain!.Mode);
    }
    private static (AuthoritySimulation, Actor) Setup(int corpseType = 0, double ceiling = 512)
    {
        var things = new List<LevelThing> { new() { Type = 64 }, new() { Type = 1, X = 400 } };
        if (corpseType != 0) things.Add(new LevelThing { Type = corpseType, X = 50 });
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = things, Lines = new List<LevelLine>(),
            Sectors = [new LevelSector { CeilingHeight = ceiling }], Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        return (sim, sim.Actors.First(actor => actor.DoomEdNum == 64));
    }
}
