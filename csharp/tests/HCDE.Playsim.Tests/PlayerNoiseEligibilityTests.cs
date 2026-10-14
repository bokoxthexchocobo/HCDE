using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerNoiseEligibilityTests
{
    [Fact]
    public void NativePlayerFactoryDefaultsToFriendly()
    {
        Assert.True(new PlayerPawn().Friendly);
        var (_, _, player, _) = Room(); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    public void DirectPlayerNoiseRequiresFriendlyAndNotNoTarget(bool friendly, bool noTarget, bool wakes)
    {
        var (sim, actor, player, _) = Room(); player.Friendly = friendly; player.NoTarget = noTarget;
        actor.Brain!.SetChaseThreshold(100, false); actor.Brain.Hear(sim, actor, player);
        Assert.Equal(wakes ? player.Id : (uint?)null, actor.TargetId); Assert.Equal(0, actor.Brain.Threshold);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void RejectedRememberedPlayerStopsGoalFallback(bool friendly, bool noTarget)
    {
        var (sim, actor, player, goal) = Room(); player.Friendly = friendly; player.NoTarget = noTarget;
        actor.LastHeardTargetId = player.Id; actor.GoalId = goal.Id; actor.ReactionTime = 0;
        actor.Brain!.Tick(sim, actor); Assert.Null(actor.TargetId); Assert.Equal(MonsterMode.Idle, actor.Brain.Mode);
        Assert.Equal(player.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void ExistingPlayerTargetIsNotDroppedForLosingFriendlyFlag()
    {
        var (sim, actor, player, _) = Room(); actor.SetTargetPointer(player); player.Friendly = false;
        actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false); Assert.Equal(player.Id, actor.TargetId);
    }

    [Fact]
    public void RestoredRejectedNoiseStillStopsFallback()
    {
        var (sim, actor, player, goal) = Room(); player.Friendly = false; actor.LastHeardTargetId = player.Id; actor.GoalId = goal.Id;
        var saved = SimSavegame.Write(sim, true); player.Friendly = true; SimSavegame.Apply(sim, saved);
        actor.Brain!.Tick(sim, actor); Assert.Null(actor.TargetId); Assert.Equal(player.Id, actor.LastHeardTargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, PlayerPawn Player, Actor Goal) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }, new LevelThing { Type = 9024, Y = 300 }] });
        return (sim, sim.Actors[0], sim.Players.Single(), sim.Actors.Single(a => a.ClassDoomEdNum == 9024));
    }
}
