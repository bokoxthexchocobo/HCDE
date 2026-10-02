using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkGravityFactorTests
{
    [Theory]
    [InlineData(-4, false)]
    [InlineData(0, false)]
    [InlineData(4, false)]
    [InlineData(-4, true)]
    [InlineData(0, true)]
    [InlineData(4, true)]
    public void IceDebrisUsesOneEighthGravityAndRespectsNoGravity(int initialVelocity, bool noGravity)
    {
        var sim = Room();
        var chunk = new IceChunkActor(10) { Simulation = sim, Level = sim.Level,
            Z = Fixed.FromInt(100), VelocityZ = Fixed.FromInt(initialVelocity), NoGravity = noGravity,
            OnGround = false };
        chunk.Tick();
        Assert.Equal(initialVelocity - (noGravity ? 0 : 0.125), chunk.VelocityZ.ToDouble());
        chunk.Tick();
        Assert.Equal(initialVelocity - (noGravity ? 0 : 0.25), chunk.VelocityZ.ToDouble());
        Assert.Equal(8, chunk.RemainingTics);
    }

    [Fact]
    public void OrdinaryActorRetainsExistingUnitGravity()
    {
        var sim = Room(); var actor = sim.AddBot(64, 0);
        actor.Z = Fixed.FromInt(100); actor.VelocityZ = Fixed.FromInt(4); actor.OnGround = false;
        ActorPhysics.Step(sim, actor);
        Assert.Equal(3, actor.VelocityZ.ToDouble()); Assert.Equal(103, actor.Z.ToDouble());
    }

    [Fact]
    public void RestingIceChunkDoesNotAcquireDownwardVelocity()
    {
        var sim = Room(); var chunk = new IceChunkActor(10) { Simulation = sim, Level = sim.Level };
        chunk.Tick(); Assert.Equal(0, chunk.VelocityZ.ToDouble()); Assert.Equal(0, chunk.Z.ToDouble());
        Assert.True(chunk.OnGround);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
