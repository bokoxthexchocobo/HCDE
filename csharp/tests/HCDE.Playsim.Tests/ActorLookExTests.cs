using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorLookExTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DontChaseGoalControlsPatrolFallback(bool dontChase)
    {
        var sim = Room(); var actor = sim.Actors[0]; sim.Players.Single().NoTarget = true;
        var goal = sim.Actors.Single(a => a.ClassDoomEdNum == PatrolMarkers.PointType);
        actor.GoalId = goal.Id; actor.ReactionTime = 0;
        ActorLookActions.LookEx(actor, dontChase ? ActorLookFlags.DontChaseGoal : 0);
        Assert.Equal(dontChase ? null : (uint?)goal.Id, actor.TargetId);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(20, false)]
    public void GoalWaitTimestampPreventsPrematureWake(int delay, bool targetsGoal)
    {
        var sim = Room(); var actor = sim.Actors[0]; sim.Players.Single().NoTarget = true;
        var goal = sim.Actors.Single(a => a.ClassDoomEdNum == PatrolMarkers.PointType);
        actor.Special = 229; actor.SpecialArgs[1] = 9; actor.SpecialArgs[2] = delay;
        ActorLookActions.LookEx(actor);
        Assert.Equal(0, actor.Special); Assert.Equal(goal.Id, actor.GoalId);
        Assert.Equal(targetsGoal ? goal.Id : (uint?)null, actor.TargetId);
    }

    [Fact]
    public void ExplicitWakeStateOverridesDefaultSeeState()
    {
        var actor = Room().Actors[0];
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSeeSound, seeState: 2);
        Assert.Equal(2, actor.States.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoJumpControlsStateChangeWithoutPreventingVisualAcquisition(bool noJump)
    {
        var sim = Room(); var actor = sim.Actors[0]; var spawn = actor.States.Current;
        actor.Brain!.SetChaseThreshold(20, false);
        ActorLookActions.LookEx(actor, noJump ? ActorLookFlags.NoJump : 0);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId); Assert.Equal(0, actor.Brain.Threshold);
        Assert.Equal(noJump, actor.States.Current == spawn);
    }

    [Theory]
    [InlineData(ActorLookFlags.NoSightCheck, true)]
    [InlineData(ActorLookFlags.NoSightCheck | ActorLookFlags.NoSoundCheck, false)]
    public void SoundAndSightFlagsAreIndependent(ActorLookFlags flags, bool found)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.LastHeardTargetId = sim.Players.Single().Id;
        ActorLookActions.LookEx(actor, flags | ActorLookFlags.NoJump);
        Assert.Equal(found ? sim.Players.Single().Id : (uint?)null, actor.TargetId);
    }

    [Theory]
    [InlineData(127, false)]
    [InlineData(128, true)]
    public void HearingDistanceClearsOnlyOutOfRangeMemory(double maximum, bool heard)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.LastHeardTargetId = sim.Players.Single().Id;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoJump, maxHearDistance: maximum);
        Assert.Equal(heard ? sim.Players.Single().Id : (uint?)null, actor.TargetId);
        Assert.Equal(heard ? sim.Players.Single().Id : (uint?)null, actor.LastHeardTargetId);
    }

    [Theory]
    [InlineData(ActorLookFlags.NoJump, 1)]
    [InlineData(ActorLookFlags.NoJump | ActorLookFlags.FullVolumeSeeSound, 0)]
    [InlineData(ActorLookFlags.NoJump | ActorLookFlags.NoSeeSound, -1)]
    public void LookExSeeSoundUsesExplicitVolumeFlags(ActorLookFlags flags, int attenuation)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Boss = actor.FullVolumeSee = true;
        sim.DrainSoundRequests(); ActorLookActions.LookEx(actor, flags);
        var requests = sim.DrainSoundRequests();
        if (attenuation < 0) Assert.Empty(requests);
        else { var sound = Assert.Single(requests); Assert.Equal("test/see", sound.Sound); Assert.Equal(attenuation, sound.Attenuation); }
    }

    [Theory]
    [InlineData(128, false)]
    [InlineData(129, true)]
    public void AmbushHearingUsesStrictSightDistanceBoundaries(double maximum, bool wakes)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Ambush = true;
        actor.LastHeardTargetId = sim.Players.Single().Id; sim.DrainSoundRequests();
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoJump, maxSeeDistance: maximum);
        Assert.Equal(wakes, sim.DrainSoundRequests().Length > 0);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId);
    }

    [Fact]
    public void MissingHateesDoNotFallBackToPlayersWithoutHuntPlayers()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.TidToHate = 7; actor.Brain!.SetChaseThreshold(20, false);
        ActorLookActions.LookEx(actor);
        Assert.Equal(0, actor.Brain.Threshold); Assert.Null(actor.TargetId);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 128 },
                new LevelThing { Type = PatrolMarkers.PointType, Id = 9, X = 500 }]
        });
        sim.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "test/see" };
        sim.Actors[0].SeeState = 1;
        sim.Actors[0].States.Configure(sim.Actors[0], [new ActorFrame(-1, 0), new ActorFrame(-1, 1), new ActorFrame(-1, 2)], 0);
        return sim;
    }
}
