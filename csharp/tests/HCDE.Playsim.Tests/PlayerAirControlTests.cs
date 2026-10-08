using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerAirControlTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1.0 / 256, 1)]
    [InlineData(0.25, 0.976875)]
    [InlineData(2, 0.8122)]
    public void AirFrictionUsesNativeThresholdAndFormula(double control, double friction)
    {
        var sim = Room(control); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(64); player.OnGround = false;
        player.VelocityX = Fixed.FromInt(4); player.VelocityY = Fixed.FromInt(-2);
        sim.Tick(); Assert.Equal(Fixed.FromInt(4), player.X);
        Assert.Equal(Fixed.FromDouble(4 * friction), player.VelocityX);
        Assert.Equal(Fixed.FromDouble(-2 * friction), player.VelocityY);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AirFrictionExcludesGroundAndActiveFlight(bool flight)
    {
        var sim = Room(2); var player = sim.Players.Single(); player.Fly = player.NoGravity = flight;
        if (flight) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        player.VelocityX = Fixed.FromInt(4); sim.Tick();
        Assert.Equal(Fixed.FromDouble(4 * (flight ? ActorPhysics.FlyingFriction : ActorPhysics.GroundFriction)), player.VelocityX);
    }

    [Fact]
    public void TinyAirborneVelocityIsDampedWithoutGroundStopThreshold()
    {
        var sim = Room(2); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(64); player.OnGround = false; player.VelocityX = Fixed.FromDouble(0.03125);
        sim.Tick(); Assert.Equal(Fixed.FromDouble(0.03125 * 0.8122), player.VelocityX);
    }

    [Theory]
    [InlineData(0, false, false, 0)]
    [InlineData(0.25, false, false, 0.25)]
    [InlineData(2, false, false, 2)]
    [InlineData(0, true, false, 1)]
    [InlineData(0, false, true, 1)]
    public void ConfiguredAirControlScalesOnlyGravityDrivenAirborneThrust(double control, bool grounded, bool noGravity, double expected)
    {
        var sim = Room(control); var player = sim.Players.Single(); player.NoGravity = noGravity;
        if (!grounded) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, SideMove = 4096 }); sim.Tick();
        Assert.Equal(Fixed.FromDouble(expected), player.X);
        Assert.Equal(Fixed.FromDouble(-expected / 2), player.Y);
    }

    [Fact]
    public void ConfigurationCopiesAndSaveContinuationUsesSameAirControl()
    {
        var sim = Room(0.25); Assert.Equal(0.25, sim.Level.CopyForSimulation().AirControl);
        var player = sim.Players.Single(); player.Z = Fixed.FromInt(64); player.OnGround = false;
        var restored = Room(0.25); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
        Assert.NotEqual(Room(0).Checksum, Room(0.25).Checksum);
    }

    [Fact]
    public void NonfiniteConfigurationFailsAtSimulationStart()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Room(double.NaN));

    private static AuthoritySimulation Room(double control) => AuthoritySimulation.Start(new PlayLevel
    {
        AirControl = control, Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    }, rngSeed: 42);
}
