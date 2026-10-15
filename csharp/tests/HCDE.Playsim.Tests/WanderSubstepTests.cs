using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WanderSubstepTests
{
    [Fact]
    public void FeetAtFloorAllowWalkingWithoutManagedGroundFlag()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.OnGround = false;
        actor.MoveCount = 10; actor.ReactionTime = 0;
        var x = actor.X.Raw;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn);
        Assert.Equal(x + actor.MovementSpeed.Raw, actor.X.Raw);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FastStepChecksInterveningActorsAndPreservesPartialProgress(bool excluded)
    {
        var sim = Room(); var actor = sim.Actors[0]; var blocker = sim.Actors[1]; blocker.NoBlockmap = excluded;
        actor.MovementSpeed = Fixed.FromInt(256); actor.MoveCount = 10; actor.ReactionTime = 0;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        Assert.Equal(9, actor.MoveCount);
        if (excluded) Assert.Equal(Fixed.FromInt(320), actor.X);
        else { Assert.True(actor.X.ToDouble() > 64); Assert.True(actor.X.ToDouble() < blocker.X.ToDouble()); }
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void UnblockedFastStepReachesExactEndpoint(int direction)
    {
        var sim = Room(); var actor = sim.Actors[0]; sim.Actors[1].NoBlockmap = true;
        actor.MovementSpeed = Fixed.FromInt(128); actor.MoveCount = 10; actor.ReactionTime = 0;
        actor.MoveDirection = (byte)direction;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.NoDirectionTurn);
        var dx = direction == 0 ? 128 : direction == 1 ? 128 * Math.Sqrt(0.5) : 0;
        var dy = direction == 2 ? 128 : direction == 1 ? 128 * Math.Sqrt(0.5) : 0;
        Assert.Equal(Fixed.FromDouble(64 + dx), actor.X); Assert.Equal(Fixed.FromDouble(64 + dy), actor.Y);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlastedBypassPrecedesDirectionAndGroundValidation(bool blasted)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Blasted = blasted; actor.OnGround = false; actor.Z = Fixed.FromInt(10);
        actor.MoveDirection = 255; actor.MoveCount = 10; actor.ReactionTime = 0;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn);
        Assert.Equal(blasted ? 255 : 8, actor.MoveDirection);
        Assert.Equal(blasted ? 9 : 14, actor.MoveCount);
        Assert.Equal(blasted, sim.CaptureState().NewChaseDirRandomState is null);
        Assert.Equal(Fixed.FromInt(64), actor.X);
    }

    [Fact]
    public void PartialBlockedStepAndNextActionSurviveSave()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.MovementSpeed = Fixed.FromInt(256);
        actor.MoveCount = 10; actor.ReactionTime = 0;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        var saved = SimSavegame.Write(sim, true);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64 }, new LevelThing { Type = 3004, X = 192, Y = 64 }]
    });
}
