using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WanderFrictionTests
{
    [Theory]
    [InlineData(false, false, 8)]
    [InlineData(true, false, 4.0625)]
    [InlineData(true, true, 8)]
    public void MudWalkingRequiresCompatibilityAndHonorsNoFriction(bool compatible, bool noFriction, double distance)
    {
        var sim = Room(0.5, 32.0 / 65536, compatible); var actor = sim.Actors.Single();
        actor.NoFriction = noFriction; Walk(actor);
        Assert.Equal(Fixed.FromDouble(distance), actor.X); Assert.Equal(default, actor.VelocityX);
    }

    [Theory]
    [InlineData(0, 4.0625)]
    [InlineData(15000, 4.0625)]
    [InlineData(15001, 4.125)]
    [InlineData(30001, 4.25)]
    [InlineData(60001, 4.5)]
    public void MudUsesExistingMomentumThresholds(int velocity, double distance)
    {
        var sim = Room(0.5, 32.0 / 65536); var actor = sim.Actors.Single();
        actor.VelocityX = new Fixed(velocity); Walk(actor);
        Assert.Equal(Fixed.FromDouble(distance), actor.X); Assert.Equal(new Fixed(velocity), actor.VelocityX);
    }

    [Fact]
    public void IceConvertsSuccessfulStepToAddedMomentum()
    {
        var sim = Room(0.98, 1024.0 / 65536); var actor = sim.Actors.Single();
        actor.VelocityX = Fixed.FromInt(3); actor.VelocityY = Fixed.FromInt(2); actor.InFloat = true;
        Walk(actor);
        Assert.Equal(default, actor.X); Assert.Equal(Fixed.FromInt(4), actor.VelocityX);
        Assert.Equal(Fixed.FromInt(2), actor.VelocityY); Assert.False(actor.InFloat);
    }

    [Fact]
    public void BlockedIceMoveDoesNotAddMomentum()
    {
        var sim = Room(0.98, 1024.0 / 65536); var actor = sim.Actors.Single();
        sim.AddBot(40, 0); actor.MovementSpeed = Fixed.FromInt(80);
        Walk(actor);
        Assert.Equal(default, actor.VelocityX);
    }

    [Fact]
    public void SavedIceMomentumContinuesIdentically()
    {
        var sim = Room(0.98, 1024.0 / 65536); var actor = sim.Actors.Single();
        Walk(actor); var saved = SimSavegame.Write(sim, true);
        Walk(actor); var velocity = actor.VelocityX;
        SimSavegame.Apply(sim, saved); Walk(actor);
        Assert.Equal(velocity, actor.VelocityX); Assert.Equal(default, actor.X);
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 10;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static AuthoritySimulation Room(double friction, double factor, bool compatible = true)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Surface", Friction: friction, MoveFactor: factor)],
            DefaultTerrain = "Surface", Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }]
        }, compat: compatible ? CompatSurface.MbfMonsterMove : CompatSurface.None);
        sim.Actors.Single().MovementSpeed = Fixed.FromInt(8);
        return sim;
    }
}
