using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorContactPushTests
{
    [Theory]
    [InlineData(true, false, 0.25, 6)]
    [InlineData(true, false, 0.5, 12)]
    [InlineData(true, false, 0, 0)]
    [InlineData(true, false, -0.5, -12)]
    [InlineData(false, false, 0.25, 0)]
    [InlineData(true, true, 0.25, 0)]
    public void ContactTransfersHorizontalMomentumOncePerMovement(bool pushable, bool cannotPush, double factor, double expected)
    {
        var (sim, player, target) = Room(); target.Pushable = pushable; player.CannotPush = cannotPush;
        target.PushFactor = factor; target.VelocityZ = Fixed.FromInt(3); player.VelocityX = Fixed.FromInt(24);
        ActorPhysics.Step(sim, player);
        Assert.Equal(expected, target.VelocityX.ToDouble()); Assert.Equal(0, target.VelocityY.Raw);
        Assert.Equal(3, target.VelocityZ.ToDouble()); Assert.Null(player.MovementPushContacts);
    }

    [Fact]
    public void SolidPushableStillBlocksMoverAndPlaysPushSound()
    {
        var (sim, player, target) = Room(); target.Solid = true; player.VelocityX = Fixed.FromInt(24);
        ActorPhysics.Step(sim, player);
        Assert.Equal(6, target.VelocityX.ToDouble()); Assert.True(player.X.ToDouble() < 24);
        Assert.Equal("push/test", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Fact]
    public void MovementBatchesAreIndependentEvenWithinSameTic()
    {
        var (sim, player, target) = Room(); player.VelocityX = Fixed.FromInt(24);
        ActorPhysics.Step(sim, player); Assert.Equal(6, target.VelocityX.ToDouble());
        player.VelocityX = Fixed.FromInt(24); ActorPhysics.Step(sim, player);
        Assert.Equal(12, target.VelocityX.ToDouble()); Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ThroughActorsBlockmapAndHeightClearanceSkipPush(int mode)
    {
        var (sim, player, target) = Room(); player.VelocityX = Fixed.FromInt(24);
        if (mode == 0) player.ThruActors = true;
        if (mode == 1) target.NoBlockmap = true;
        if (mode == 2) target.Z = Fixed.FromInt(128);
        ActorPhysics.Step(sim, player); Assert.Equal(0, target.VelocityX.Raw); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void WorldHapticsPreferenceDoesNotSuppressPushPhysicsOrSound()
    {
        var (sim, player, target) = Room(); sim.WorldHapticsEnabled = false; var calls = 0;
        sim.RumbleRequested += _ => calls++; player.VelocityX = Fixed.FromInt(24); ActorPhysics.Step(sim, player);
        Assert.Equal(6, target.VelocityX.ToDouble()); Assert.Equal(0, calls); Assert.Single(sim.PendingSoundRequests);
    }

    [Fact]
    public void SaveAndRestoreResumeSamePushWithoutPersistingMovementContext()
    {
        var (sim, player, target) = Room(); player.VelocityX = Fixed.FromInt(24);
        var saved = SimSavegame.Write(sim, true); var (restored, loaded, pushed) = Room(); SimSavegame.Apply(restored, saved);
        ActorPhysics.Step(sim, player); ActorPhysics.Step(restored, loaded);
        Assert.Equal(6, pushed.VelocityX.ToDouble()); Assert.Equal(target.VelocityX, pushed.VelocityX);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Null(loaded.MovementPushContacts);
    }

    [Fact]
    public void ExceptionFromPresentationSubscriberStillClearsMovementContext()
    {
        var (sim, player, _) = Room(); player.VelocityX = Fixed.FromInt(24);
        sim.RumbleRequested += _ => throw new InvalidOperationException("test callback");
        Assert.Throws<InvalidOperationException>(() => ActorPhysics.Step(sim, player)); Assert.Null(player.MovementPushContacts);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
        var target = sim.AddBot(40, 0); target.Brain = null; target.Pushable = true; target.Solid = false;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, sim.Players.Single(), target);
    }
}
