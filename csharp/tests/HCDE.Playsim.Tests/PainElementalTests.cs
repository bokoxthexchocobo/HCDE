using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PainElementalTests
{
    [Fact]
    public void AttackSpawnsChargingSoulAfterFifteenTics()
    {
        var (sim, parent) = Setup();
        Windup(sim, parent);
        for (var i = 0; i < 14; i++) sim.Tick();
        Assert.Empty(Souls(sim));
        sim.Tick();
        var soul = Assert.Single(Souls(sim));
        Assert.True(soul.Brain!.Charging);
        Assert.Equal(sim.Players.Single().Id, soul.Brain.TargetId);
        Assert.Equal(100, soul.Health);
        Assert.Equal(16, soul.Radius.ToDouble());
        Assert.True(parent.Solid);
        Assert.True(soul.X.ToDouble() > parent.X.ToDouble() + parent.Radius.ToDouble());
    }

    [Fact]
    public void DeathBurstRunsOnceAtThirtyTwoTics()
    {
        var (sim, parent) = Setup();
        parent.Health = 0;
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.Empty(Souls(sim));
        sim.Tick();
        Assert.Equal(3, Souls(sim).Count());
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.Equal(3, Souls(sim).Count());
        Assert.Equal(3, Souls(sim).Select(soul => soul.Id).Distinct().Count());
    }

    [Fact]
    public void SpawnPathCannotSkipWallAndRestoresParentSolidity()
    {
        var (sim, parent) = Setup();
        Windup(sim, parent);
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine
            { X1 = 50, X2 = 50, Y1 = -200, Y2 = 200, SideBack = -1 });
        for (var i = 0; i < 15; i++) sim.Tick();
        var failed = Assert.Single(Souls(sim));
        Assert.True(failed.IsDead); Assert.Equal(1, failed.DeathCount);
        Assert.False(failed.Brain!.Charging); Assert.False(failed.NoTeleport);
        Assert.True(parent.Solid);
    }

    [Fact]
    public void LowCeilingRejectsSpawn()
    {
        var (sim, parent) = Setup(63);
        Windup(sim, parent);
        for (var i = 0; i < 15; i++) sim.Tick();
        Assert.Empty(Souls(sim));
        Assert.True(parent.Solid);
    }

    [Fact]
    public void InvasionWaitsForDeathBurstThenCountsChildrenUntilVictory()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 800 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        sim.Players.Single().Invulnerable = true;
        sim.Invasion.Enabled = true;
        sim.Invasion.Configure(1, 0, 35);
        sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>());
        parent.Brain = MonsterBrain.ForType(71);
        parent.Health = 0;
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase);
        Assert.Equal(0, sim.Invasion.ActiveMonsters);
        sim.Tick();
        Assert.Equal(3, sim.Invasion.ActiveMonsters);
        Assert.Equal(4, sim.Invasion.Spawned);
        Assert.Equal(1, sim.Invasion.Cleared);
        foreach (var soul in Souls(sim)) soul.Health = 0;
        sim.Tick();
        Assert.Equal(InvasionPhase.Victory, sim.Invasion.Phase);
        Assert.Equal(0, sim.Invasion.ActiveMonsters);
        Assert.Equal(4, sim.Invasion.Cleared);
        Assert.Equal(1, sim.Invasion.Wave);
    }

    [Fact]
    public void BlockedDeathBurstDoesNotLeaveInvasionWaitingForever()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 800 }],
            Sectors = [new LevelSector { CeilingHeight = 63 }],
        });
        sim.Invasion.Enabled = true; sim.Invasion.Configure(1, 0, 35); sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>());
        parent.Brain = MonsterBrain.ForType(71); parent.Health = 0;
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.Empty(Souls(sim));
        Assert.Equal(InvasionPhase.Victory, sim.Invasion.Phase);
        Assert.Equal(1, sim.Invasion.Spawned);
        Assert.Equal(1, sim.Invasion.Cleared);
    }

    [Fact]
    public void AttackAndDeathSpawningReplayDeterministically()
    {
        var (left, a) = Setup(); var (right, b) = Setup();
        for (var i = 0; i < 100; i++)
        {
            if (i == 30) { a.Health = 0; b.Health = 0; }
            left.Tick(); right.Tick();
            Assert.Equal(left.Checksum, right.Checksum);
            Assert.Equal(left.Actors.Count, right.Actors.Count);
        }
    }

    private static IEnumerable<Actor> Souls(AuthoritySimulation sim) => sim.Actors.Where(actor => actor.DoomEdNum == 3006);
    private static void Windup(AuthoritySimulation sim, Actor parent)
    {
        for (var i = 0; i < 20 && parent.Brain!.Mode != MonsterMode.Windup; i++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, parent.Brain!.Mode);
    }
    private static (AuthoritySimulation, Actor) Setup(double ceiling = 512)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 71 }, new LevelThing { Type = 1, X = 800 }],
            Lines = new List<LevelLine>(),
            Sectors = [new LevelSector { CeilingHeight = ceiling }],
            Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        sim.Players.Single().Invulnerable = true;
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == 71));
    }
}
