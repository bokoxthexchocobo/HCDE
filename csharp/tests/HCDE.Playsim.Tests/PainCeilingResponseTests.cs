using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class PainCeilingResponseTests
{
    [Theory]
    [InlineData(63, true, true)]
    [InlineData(64, true, false)]
    [InlineData(65, true, false)]
    [InlineData(63, false, true)]
    public void CeilingRejectionThrustsOnlyFloatingParent(double ceiling, bool floating, bool rejected)
    {
        var sim = Room(ceiling); var parent = Assert.Single(sim.Actors); parent.Floating = floating;
        var soul = sim.SpawnLostSoul(parent, null, 0);
        Assert.Equal(rejected, soul == null);
        Assert.Equal(rejected && floating ? -2 : 0, parent.VelocityZ.ToDouble());
        Assert.Equal(rejected && floating, parent.VerticalFriction);
        Assert.Equal(rejected && floating, parent.InFloat);
    }
    [Theory]
    [InlineData(0.249, false)]
    [InlineData(0.25, true)]
    [InlineData(-0.25, true)]
    [InlineData(-2, true)]
    public void VerticalFrictionUsesStrictStopThreshold(double speed, bool active)
    {
        var sim = Room(512); var parent = Assert.Single(sim.Actors); parent.Brain = null;
        parent.Z = Fixed.FromInt(100); parent.OnGround = false;
        parent.VelocityZ = Fixed.FromDouble(speed); parent.VerticalFriction = true;
        var quantized = parent.VelocityZ.ToDouble();
        sim.Tick();
        Assert.Equal(active, parent.VerticalFriction);
        var expected = active ? Fixed.FromDouble(quantized * ActorPhysics.GroundFriction).ToDouble() : 0;
        Assert.Equal(expected, parent.VelocityZ.ToDouble()); Assert.Equal(100 + expected, parent.Z.ToDouble());
    }
    [Fact]
    public void FrictionDoesNotDampDeadActorsAndFlagsAffectChecksum()
    {
        var sim = Room(512); var parent = Assert.Single(sim.Actors); parent.Brain = null;
        var other = Room(512); Assert.Single(other.Actors).Brain = null;
        parent.InFloat = true; parent.VerticalFriction = true;
        sim.Tick(); other.Tick(); Assert.NotEqual(other.Checksum, sim.Checksum);
        parent.VerticalFriction = true; parent.Health = 0; parent.NoGravity = true; parent.Z = Fixed.FromInt(100); parent.VelocityZ = Fixed.FromInt(-2);
        sim.Tick(); Assert.Equal(-2, parent.VelocityZ.ToDouble()); Assert.True(parent.VerticalFriction);
    }
    private static AuthoritySimulation Room(double ceiling) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = ceiling }], Things = [new LevelThing { Type = 71 }] });
}