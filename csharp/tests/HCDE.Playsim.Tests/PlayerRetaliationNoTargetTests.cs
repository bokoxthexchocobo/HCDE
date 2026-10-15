using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerRetaliationNoTargetTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, true)]
    [InlineData(2, false, true)]
    [InlineData(0, true, false)]
    [InlineData(1, true, false)]
    [InlineData(2, true, false)]
    public void PlayerNoTargetUsesHateAndHostilityExceptions(int exception, bool noHatePlayers, bool eligible)
    {
        var (sim, hunter, player) = Room(exception); hunter.NoHatePlayers = noHatePlayers;
        Assert.Equal(eligible, hunter.OkayToSwitchTarget(player));
        Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    public void DamageWakeupPreservesManagedPlayerCheatGuard(int exception, bool retaliates)
    {
        var (_, hunter, player) = Room(exception);
        Assert.Equal(1, ActorDamage.Apply(hunter, 1, player, DamageFlags.NoPain).HealthLost);
        Assert.Equal(retaliates ? player.Id : (uint?)null, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NeverTargetStillRejectsPlayerWithEitherException(bool hostile)
    {
        var (_, hunter, player) = Room(hostile ? 2 : 1); player.NeverTarget = true;
        Assert.False(hunter.OkayToSwitchTarget(player));
        ActorDamage.Apply(hunter, 1, player, DamageFlags.NoPain);
        Assert.Null(hunter.TargetId);
    }

    [Fact]
    public void SavedHatedPlayerPredicateAndCheatGuardReplayExactly()
    {
        var (sim, hunter, player) = Room(1); var saved = SimSavegame.Write(sim, true);
        Assert.True(hunter.OkayToSwitchTarget(player));
        ActorDamage.Apply(hunter, 1, player, DamageFlags.NoPain);
        Assert.Null(hunter.TargetId);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.True(hunter.OkayToSwitchTarget(player));
        ActorDamage.Apply(hunter, 1, player, DamageFlags.NoPain);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room(int exception)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 128 }],
        });
        var hunter = sim.AddBot(0, 0); hunter.Health = 1000; hunter.NoPain = true;
        hunter.SetChaseThreshold(0, false);
        var player = sim.Players.Single(); player.NoTarget = true;
        player.Friendly = false;
        if (exception == 1) { hunter.TidToHate = 7; player.ThingId = 7; }
        else if (exception == 2) player.Friendly = true;
        return (sim, hunter, player);
    }
}
