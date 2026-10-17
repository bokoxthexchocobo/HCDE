using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RipperPushHistoryTests
{
    [Theory]
    [InlineData(true, false, 0.25, true, 0.5)]
    [InlineData(true, false, 0, true, 0)]
    [InlineData(true, false, -0.5, true, -1)]
    [InlineData(false, false, 0.25, false, 0)]
    [InlineData(true, true, 0.25, false, 0)]
    public void EligibleRipperPushUpdatesHistoryWithoutOrdinaryPushSound(bool pushable, bool cannotPush, double factor, bool history, double velocity)
    {
        var (sim, missile, target) = Room(); target.Pushable = pushable; missile.CannotPush = cannotPush;
        target.PushFactor = factor; sim.DrainSoundRequests(); missile.Tick();
        Assert.Equal(history, target.LastPushFromMovement); Assert.Equal(velocity, target.VelocityX.ToDouble());
        Assert.DoesNotContain(sim.PendingSoundRequests, request => request.Sound == "push/test");
    }

    [Fact]
    public void RipperPushPermitsNextAiPushThenAiZeroStampSuppressesRepeat()
    {
        var (sim, missile, target) = Room(); missile.Tick(); var mover = AiMover(sim);
        Assert.True(target.LastPushFromMovement);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(2.5, target.VelocityX.ToDouble());
        Assert.False(target.LastPushFromMovement);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(2.5, target.VelocityX.ToDouble());
    }

    [Fact]
    public void PostRipperHistoryRestoresNextAiPushOutcome()
    {
        var (sim, missile, target) = Room(); missile.Tick(); var mover = AiMover(sim);
        var saved = SimSavegame.Write(sim, true);
        Assert.True(ActorWanderActions.Move(sim, mover)); var expectedVelocity = target.VelocityX;
        sim.RestoreState(sim.CaptureState()); var expectedChecksum = sim.Checksum;
        SimSavegame.Apply(sim, saved); Assert.True(target.LastPushFromMovement);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(expectedVelocity, target.VelocityX);
        sim.RestoreState(sim.CaptureState()); Assert.Equal(expectedChecksum, sim.Checksum);
    }

    private static Actor AiMover(AuthoritySimulation sim)
    {
        var mover = sim.AddBot(0, 0, 3004); mover.Brain = null;
        mover.MoveDirection = 0; mover.MovementSpeed = Fixed.FromInt(8); mover.VelocityX = Fixed.FromInt(8);
        return mover;
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Solid = false; target.Invulnerable = true;
        target.Pushable = true; target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20); missile.Rip = true;
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
