using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterContactPushTests
{
    [Fact]
    public void FreshZeroPushStampSuppressesOrdinaryAiPush()
    {
        var (sim, mover, target) = Room();
        Assert.True(ActorWanderActions.Move(sim, mover));
        Assert.Equal(0, target.VelocityX.Raw); Assert.False(target.LastPushFromMovement);
        Assert.Empty(sim.PendingSoundRequests); Assert.Null(mover.MovementPushContacts); Assert.False(mover.AiPushContext);
    }

    [Fact]
    public void MovementPushEnablesOneAiPushThenZeroStampSuppressesRepeats()
    {
        var (sim, mover, target) = Room(); ActorPhysics.Step(sim, mover);
        Assert.Equal(2, target.VelocityX.ToDouble()); Assert.True(target.LastPushFromMovement);
        mover.VelocityX = Fixed.FromInt(8);
        Assert.True(ActorWanderActions.Move(sim, mover));
        Assert.Equal(4, target.VelocityX.ToDouble()); Assert.False(target.LastPushFromMovement);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(4, target.VelocityX.ToDouble());
    }

    [Fact]
    public void ZeroVelocityAiStepStillPlaysPushSoundWhenStampChanges()
    {
        var (sim, mover, target) = Room(); target.LastPushFromMovement = true; mover.VelocityX = default;
        Assert.True(ActorWanderActions.Move(sim, mover));
        Assert.Equal(0, target.VelocityX.Raw); Assert.False(target.LastPushFromMovement);
        Assert.Equal("push/test", Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AiAttemptHandlesSolidBlockingWithoutBypassingCannotPush(bool cannotPush)
    {
        var (sim, mover, target) = Room(); target.Solid = true; target.LastPushFromMovement = true;
        mover.CannotPush = cannotPush;
        Assert.False(ActorWanderActions.Move(sim, mover));
        Assert.Equal(cannotPush ? 0 : 2, target.VelocityX.ToDouble());
        Assert.Equal(cannotPush, target.LastPushFromMovement);
    }

    [Fact]
    public void FitProbeDoesNotConsumePushStampOrTransferMomentum()
    {
        var (sim, mover, target) = Room(); target.LastPushFromMovement = true;
        ActorPhysics.FitsAtHeight(sim, mover, mover.Height.ToDouble());
        Assert.True(target.LastPushFromMovement); Assert.Equal(0, target.VelocityX.Raw); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void SavedMovementStampPreservesNextAiPushAndOlderSaveClearsIt()
    {
        var (sim, mover, target) = Room(); var older = SimSavegame.Write(sim, true);
        ActorPhysics.Step(sim, mover); mover.VelocityX = Fixed.FromInt(8);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(320, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var (restored, loaded, pushed) = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(pushed.LastPushFromMovement); Assert.Equal(saved, SimSavegame.Write(restored, true));
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.True(ActorWanderActions.Move(restored, loaded));
        Assert.Equal(target.VelocityX, pushed.VelocityX); Assert.False(pushed.LastPushFromMovement);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState()); Assert.Equal(sim.Checksum, restored.Checksum);
        SimSavegame.Apply(restored, older); Assert.False(pushed.LastPushFromMovement);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidPushHistoryArchiveIsRejected(int value)
    {
        var (sim, _, target) = Room(); target.LastPushFromMovement = true;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-pushhistory-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3004, X = 32 }],
        });
        var mover = sim.Actors[0]; var target = sim.Actors[1]; mover.Brain = target.Brain = null;
        mover.MoveDirection = 0; mover.MovementSpeed = Fixed.FromInt(8); mover.VelocityX = Fixed.FromInt(8);
        target.Pushable = true; target.Solid = false;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
