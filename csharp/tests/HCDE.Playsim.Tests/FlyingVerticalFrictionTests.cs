using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FlyingVerticalFrictionTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(0.125)]
    public void AirborneFlyingPlayerMovesBeforeDamping(double velocity)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = true; player.Z = Fixed.FromInt(100); player.VelocityZ = Fixed.FromDouble(velocity);
        ActorPhysics.Step(sim, player);
        Assert.Equal(100 + velocity, player.Z.ToDouble());
        Assert.Equal(Fixed.FromDouble(velocity * ActorPhysics.FlyingFriction), player.VelocityZ);
    }

    [Fact]
    public void NoGravityMonsterRetainsVerticalVelocity()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.NoGravity = true;
        actor.Z = Fixed.FromInt(100); actor.VelocityZ = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(104, actor.Z.ToDouble()); Assert.Equal(4, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void ExactFloorLandingClearsDownwardVelocity()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true;
        player.Z = Fixed.FromInt(4); player.VelocityZ = Fixed.FromInt(-4);
        ActorPhysics.Step(sim, player);
        Assert.Equal(default, player.Z); Assert.Equal(default, player.VelocityZ); Assert.True(player.OnGround);
    }

    [Fact]
    public void SaveRestoreContinuesDampedMotion()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true;
        player.Z = Fixed.FromInt(100); player.VelocityZ = Fixed.FromInt(4);
        ActorPhysics.Step(sim, player); var previousZ = player.Z; var velocity = player.VelocityZ;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state); var loaded = restored.Players.Single();
        ActorPhysics.Step(sim, player); ActorPhysics.Step(restored, loaded);
        Assert.Equal(Fixed.FromDouble(previousZ.ToDouble() + velocity.ToDouble()), player.Z);
        Assert.Equal(Fixed.FromDouble(velocity.ToDouble() * ActorPhysics.FlyingFriction), player.VelocityZ);
        Assert.Equal(player.Z, loaded.Z); Assert.Equal(player.VelocityZ, loaded.VelocityZ);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
