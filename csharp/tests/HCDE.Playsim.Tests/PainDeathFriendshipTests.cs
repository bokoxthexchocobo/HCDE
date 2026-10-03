using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PainDeathFriendshipTests
{
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    public void DeathBurstUsesTargetFriendshipEvenWhenTargetIsDead(bool friend, bool deadTarget, bool remainsFriendly)
    {
        var (sim, parent, target) = Room();
        target.Friendly = friend;
        parent.Brain!.SetTargetThingId(sim, 7);
        if (deadTarget) target.Health = 0;
        parent.Health = 0;
        Burst(sim, parent);
        Assert.Equal(remainsFriendly, parent.Friendly);
        foreach (var soul in Souls(sim))
        {
            Assert.Equal(remainsFriendly, soul.Friendly);
            Assert.Equal(1, soul.FriendPlayer);
            Assert.Equal(42, soul.TidToHate);
            Assert.True(soul.NoHatePlayers);
        }
    }

    [Fact]
    public void FriendshipIsCheckedAtBurstTime()
    {
        var (sim, parent, target) = Room();
        parent.Brain!.SetTargetThingId(sim, 7);
        parent.Health = 0;
        for (var i = 0; i < 31; i++) parent.Brain.Tick(sim, parent);
        Assert.True(parent.Friendly);
        target.Friendly = true;
        parent.Brain.Tick(sim, parent);
        Assert.False(parent.Friendly);
        Assert.All(Souls(sim), soul => Assert.False(soul.Friendly));
    }

    [Fact]
    public void TargetlessDeathDoesNotSubstituteDamageSource()
    {
        var (sim, parent, target) = Room();
        target.Friendly = true;
        parent.LastDamageSourceId = target.Id;
        parent.Health = 0;
        Burst(sim, parent);
        Assert.True(parent.Friendly);
        foreach (var soul in Souls(sim))
        {
            Assert.True(soul.Friendly);
            Assert.Null(soul.Brain!.TargetId);
            Assert.Null(soul.LastHeardTargetId);
            Assert.False(soul.Brain.Charging);
        }
    }

    private static void Burst(AuthoritySimulation sim, Actor parent)
    {
        for (var i = 0; i < 32; i++) parent.Brain!.Tick(sim, parent);
        Assert.Equal(3, Souls(sim).Length);
    }

    private static Actor[] Souls(AuthoritySimulation sim) => sim.Actors.Where(actor => actor.DoomEdNum == 3006).ToArray();

    private static (AuthoritySimulation Sim, Actor Parent, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, X = 4096 }, new LevelThing { Type = 71 },
                new LevelThing { Type = 3001, Id = 7, X = 800 }],
        });
        var parent = sim.Actors.Single(actor => actor.DoomEdNum == 71);
        parent.Friendly = true; parent.FriendPlayer = 1; parent.TidToHate = 42; parent.NoHatePlayers = true;
        return (sim, parent, sim.Actors.Single(actor => actor.DoomEdNum == 3001));
    }
}
