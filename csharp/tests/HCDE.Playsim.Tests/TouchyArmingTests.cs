using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TouchyArmingTests
{
    [Theory]
    [InlineData(true, false, 0, true)]
    [InlineData(false, false, 0, false)]
    [InlineData(true, true, 0, false)]
    [InlineData(true, false, 1, false)]
    public void ArmingRequiresTouchyNonSentientAndZeroHorizontalVelocity(bool touchy, bool sentient, int horizontal, bool armed)
    {
        var sim = Room(); var actor = sim.AddBot(80, 0);
        actor.Brain = null; actor.Touchy = touchy; actor.SeeState = sentient ? 0 : -1;
        actor.VelocityX = Fixed.FromInt(horizontal);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(armed, actor.Armed);
    }

    [Fact]
    public void HorizontallyStationaryFallingActorArmsBeforeImpactAndDies()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0);
        actor.Brain = null; actor.SeeState = -1; actor.Touchy = true;
        actor.Z = Fixed.FromInt(8); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-16);
        ActorPhysics.Step(sim, actor);
        Assert.True(actor.IsDead);
        Assert.False(actor.Armed);
    }

    [Fact]
    public void FrictionStopArmsMineAndMotionDoesNotDisarmIt()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0);
        actor.Brain = null; actor.SeeState = -1; actor.Touchy = true;
        actor.VelocityX = Fixed.FromDouble(0.01);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(0, actor.VelocityX.Raw); Assert.True(actor.Armed);
        actor.VelocityX = Fixed.FromInt(1);
        ActorPhysics.Step(sim, actor);
        Assert.True(actor.Armed);
    }

    [Fact]
    public void AutomaticallyArmedStateSurvivesFreshSaveAndContinuation()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Touchy = true; player.SeeState = -1;
        sim.Tick(); Assert.True(player.Armed);
        var saved = SimSavegame.Write(sim, true);
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().Armed);
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
