using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WanderFloorSettleTests
{
    [Theory]
    [InlineData(24, 0)]
    [InlineData(8, 16)]
    public void SuccessfulStepSettlesOnlyWithinMaxStepHeight(int maximum, int expected)
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.MaxStepHeight = Fixed.FromInt(maximum);
        actor.InFloat = true; Walk(actor);
        Assert.Equal(40, actor.X.ToDouble()); Assert.Equal(expected, actor.Z.ToDouble()); Assert.False(actor.InFloat);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoGravityAndOnMobjSkipFloorSettlement(bool onMobj)
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.NoGravity = !onMobj; actor.OnMobj = onMobj;
        Walk(actor); Assert.Equal(40, actor.X.ToDouble()); Assert.Equal(16, actor.Z.ToDouble());
    }

    [Fact]
    public void SettlementDoesNotPushActorIntoLowerSolidObject()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.CanPass = true;
        var blocker = sim.AddBot(40, 0); blocker.Height = Fixed.FromInt(8); blocker.Brain!.Enabled = false;
        Walk(actor);
        Assert.Equal(40, actor.X.ToDouble()); Assert.Equal(16, actor.Z.ToDouble());
    }

    [Fact]
    public void SuccessfulWalkClearsInFloatWhileBlastedBypassPreservesIt()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.InFloat = true; actor.Blasted = true;
        Walk(actor); Assert.True(actor.InFloat); Assert.Equal(-40, actor.X.ToDouble());
        actor.Blasted = false; Walk(actor); Assert.False(actor.InFloat); Assert.Equal(40, actor.X.ToDouble());
    }

    [Fact]
    public void OnMobjAllowsAboveFloorWalkingWithoutGroundFlag()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.Z = Fixed.FromInt(32);
        actor.OnMobj = true; actor.OnGround = false; actor.AllowDropOff = true;
        Walk(actor); Assert.Equal(40, actor.X.ToDouble()); Assert.Equal(32, actor.Z.ToDouble());
    }

    [Fact]
    public void SettledHeightAndFloatFlagSurviveSavedContinuation()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.InFloat = true;
        Walk(actor); var saved = SimSavegame.Write(sim, true);
        actor.Z = Fixed.FromInt(16); actor.InFloat = true;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(0, actor.Z.ToDouble()); Assert.False(actor.InFloat);
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { FloorHeight = 16, CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
            Lines = geometry.Lines, Sides = geometry.Sides, Things = [new LevelThing { Type = 3004, X = -40 }]
        });
    }
}
