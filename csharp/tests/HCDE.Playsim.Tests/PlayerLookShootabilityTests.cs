using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLookShootabilityTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void PlayerDiscoveryRequiresShootableEvenWhenVulnerable(bool shootable, bool vulnerable, bool hateFallback)
    {
        var (sim, hunter, player) = Room(hateFallback);
        player.Shootable = shootable; player.Vulnerable = vulnerable;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(shootable ? player.Id : (uint?)null, hunter.TargetId);
        if (!shootable && vulnerable)
        {
            Assert.True(player.CanTakeDamage);
            var health = player.Health; ActorDamage.Apply(player, 7);
            Assert.True(player.Health < health);
        }
    }

    [Fact]
    public void BecomingShootableAllowsLaterAcquisition()
    {
        var (sim, hunter, player) = Room(false);
        player.Shootable = false; player.Vulnerable = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive); Assert.Null(hunter.TargetId);
        player.Shootable = true;
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive); Assert.Equal(player.Id, hunter.TargetId);
    }

    [Fact]
    public void NonShootablePlayerAllowsAssignedGoalFallback()
    {
        var (sim, hunter, player) = Room(false);
        player.Shootable = false; player.Vulnerable = true;
        var goal = sim.AddBot(-300, 0, 3001); hunter.GoalId = goal.Id; hunter.ReactionTime = 0;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(goal.Id, hunter.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Hunter, PlayerPawn Player) Room(bool hateFallback)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 800 }]
        });
        var hunter = sim.AddBot(0, 0);
        if (hateFallback) { hunter.TidToHate = 99; hunter.HuntPlayers = true; }
        return (sim, hunter, sim.Players.Single());
    }
}
