using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeadBrainMomentumTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullTickPreservesDeadActorMovementAndAppliesFloorFriction(bool dontCorpse)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        actor.DontCorpse = dontCorpse;
        ActorDamage.Apply(actor, 1000);
        actor.VelocityX = Fixed.FromInt(4); actor.VelocityY = Fixed.FromInt(2);
        sim.Tick();
        Assert.Equal(Fixed.FromInt(4), actor.X);
        Assert.Equal(Fixed.FromInt(2), actor.Y);
        Assert.Equal(Fixed.FromDouble(4 * ActorPhysics.GroundFriction), actor.VelocityX);
        Assert.Equal(Fixed.FromDouble(2 * ActorPhysics.GroundFriction), actor.VelocityY);
        Assert.Equal(MonsterMode.Dead, actor.Brain!.Mode);
    }

    [Fact]
    public void AirborneDeadActorKeepsHorizontalMomentum()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        ActorDamage.Apply(actor, 1000);
        actor.Z = Fixed.FromInt(64); actor.OnGround = false;
        actor.VelocityX = Fixed.FromInt(4); actor.VelocityY = Fixed.FromInt(-2);
        sim.Tick();
        Assert.Equal(Fixed.FromInt(4), actor.X);
        Assert.Equal(Fixed.FromInt(-2), actor.Y);
        Assert.Equal(Fixed.FromInt(4), actor.VelocityX);
        Assert.Equal(Fixed.FromInt(-2), actor.VelocityY);
        Assert.Equal(Fixed.FromInt(-1), actor.VelocityZ);
    }

    [Fact]
    public void FullTickKeepsCorpseSlidingAcrossPartlySupportedLedge()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var actor = sim.AddBot(1, 80, 3004);
        ActorDamage.Apply(actor, 1000);
        actor.Z = default; actor.SectorIndex = 1; actor.OnGround = true;
        actor.VelocityX = Fixed.FromDouble(0.5);
        sim.Tick();
        Assert.Equal(Fixed.FromDouble(1.5), actor.X);
        Assert.Equal(Fixed.FromDouble(0.5), actor.VelocityX);
    }

    [Fact]
    public void RestoredCorpseMomentumSurvivesNextFullTick()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        ActorDamage.Apply(actor, 1000);
        actor.VelocityX = Fixed.FromInt(4); actor.VelocityY = Fixed.FromInt(2);
        var bytes = SimSavegame.Write(sim);
        actor.VelocityX = actor.VelocityY = default;
        SimSavegame.Apply(sim, bytes);
        sim.Tick();
        Assert.Equal(Fixed.FromInt(4), actor.X);
        Assert.Equal(Fixed.FromInt(2), actor.Y);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }],
    });
}
