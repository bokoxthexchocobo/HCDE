using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetFrictionTests
{
    [Theory]
    [InlineData(0.98, 0.5, 0.5, 0.01)]
    [InlineData(0.5, 0.98, 0.5, 0.02)]
    [InlineData(0.98, 0.96, 0.96, 0.01)]
    public void TouchingFloorsChooseLowestFrictionAndItsMoveFactor(double left, double right, double expected, double factor)
    {
        var sim = Room(left, right); var actor = sim.Players.Single(); actor.X = Fixed.FromInt(-8);
        Assert.Equal((expected, factor), actor.GetFriction());
        var saved = SimSavegame.Write(sim, true); var fresh = Room(left, right); SimSavegame.Apply(fresh, saved);
        Assert.Equal(actor.GetFriction(), fresh.Players.Single().GetFriction());
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(15000, 1)]
    [InlineData(15001, 2)]
    [InlineData(30001, 4)]
    [InlineData(60001, 8)]
    public void QueryAppliesStrictMudVelocityBoost(int rawSpeed, int boost)
    {
        var sim = Room(0.5, 0.98); var actor = sim.Players.Single(); actor.VelocityX = new(rawSpeed);
        Assert.Equal((0.5, 0.02 * boost), actor.GetFriction());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoClip2UsesDefaultFactorsEvenOnMudOrInFlight(bool flying)
    {
        var sim = Room(0.5, 0.98); var actor = sim.Players.Single();
        actor.NoClip2 = true; actor.Fly = flying; actor.NoGravity = flying;
        Assert.Equal((ActorPhysics.GroundFriction, 2048 / 65536.0), actor.GetFriction());
    }

    [Fact]
    public void BoundaryMovementUsesQueriedDampingAfterFreshRestore()
    {
        var sim = Room(0.98, 0.5); var actor = sim.Players.Single();
        actor.X = Fixed.FromInt(-8); actor.VelocityX = Fixed.FromInt(1);
        var expected = actor.GetFriction().Friction;
        var fresh = Room(0.98, 0.5); SimSavegame.Apply(fresh, SimSavegame.Write(sim, true));
        sim.Tick(); fresh.Tick();
        Assert.Equal(Fixed.FromDouble(expected), actor.VelocityX);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void AirborneQueryIgnoresFloorAndDetachedQueryFails()
    {
        var sim = Room(0.5, 0.98); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(1);
        Assert.Equal((ActorPhysics.GroundFriction, 2048 / 65536.0), actor.GetFriction());
        Assert.Throws<InvalidOperationException>(() => new Actor().GetFriction());
    }

    private static AuthoritySimulation Room(double left, double right)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new("Left", Friction: left, MoveFactor: 0.02), new("Right", Friction: right, MoveFactor: 0.01)],
            Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Left" }, new LevelSector { CeilingHeight = 128, FloorTerrain = "Right" }],
            Lines = geometry.Lines, Sides = geometry.Sides, Things = [new LevelThing { Type = 1, X = -40 }]
        });
    }
}
