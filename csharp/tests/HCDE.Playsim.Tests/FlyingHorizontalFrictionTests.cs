using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FlyingHorizontalFrictionTests
{
    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public void ActiveFlightSelectsNativeHorizontalFriction(bool fly, bool noGravity, bool ground)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Fly = fly; player.NoGravity = noGravity;
        if (!ground) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        player.VelocityX = Fixed.FromInt(4); player.VelocityY = Fixed.FromInt(-2);
        sim.Tick();
        var friction = fly && noGravity ? ActorPhysics.FlyingFriction : ground ? ActorPhysics.GroundFriction : 1;
        Assert.Equal(4, player.X.ToDouble()); Assert.Equal(-2, player.Y.ToDouble());
        Assert.Equal(4 * friction, player.VelocityX.ToDouble());
        Assert.Equal(-2 * friction, player.VelocityY.ToDouble());
    }

    [Fact]
    public void ActiveFlightAppliesActorFrictionMultiplier()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Fly = true; player.NoGravity = true;
        player.Friction = Fixed.FromDouble(0.25); player.VelocityX = Fixed.FromInt(4);
        sim.Tick(); Assert.Equal(ActorPhysics.FlyingFriction, player.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(0.25, 32.0 / 2048)]
    [InlineData(2, 602.3529411764706 / 2048)]
    public void FlightAccelerationUsesMultipliedFrictionAndResumesFromSave(double multiplier, double scale)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Fly = player.NoGravity = true;
        player.Z = Fixed.FromInt(64); player.OnGround = false; player.Friction = Fixed.FromDouble(multiplier);
        var restored = Room(); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick(); restored.Tick(); Assert.Equal(Fixed.FromDouble(scale), player.X);
        Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    [Fact]
    public void LandingSwitchesHorizontalDampingBackToGroundFriction()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Fly = true; player.NoGravity = true;
        player.VelocityX = Fixed.FromInt(4);
        sim.QueueCommand(0, new PlayerCommand { UpMove = short.MinValue }); sim.Tick();
        Assert.False(player.NoGravity);
        Assert.Equal(4 * ActorPhysics.GroundFriction, player.VelocityX.ToDouble());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
