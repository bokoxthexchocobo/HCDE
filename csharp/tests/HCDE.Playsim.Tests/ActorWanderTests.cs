using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWanderTests
{
    [Fact]
    public void WanderFieldsAndStreamsParticipateInSavedChecksum()
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var oldHash = sim.Checksum;
        actor.MoveDirection = 3; actor.MoveCount = -7;
        sim.NextNewChaseDirByte(); sim.NextTryWalkByte(); sim.RestoreState(sim.CaptureState());
        var hash = sim.Checksum; Assert.NotEqual(oldHash, hash); var saved = SimSavegame.Write(sim, true);
        actor.MoveDirection = 0; actor.MoveCount = 0; sim.NextTryWalkByte(); sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(hash, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(3, actor.MoveDirection); Assert.Equal(-7, actor.MoveCount);
        SimSavegame.Apply(sim, old); Assert.Equal(oldHash, sim.Checksum);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(-1, -2)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void ReactionTimeIsSignedAndDelaysMovement(int initial, int expected)
    {
        var actor = Room().Actors[0]; actor.ReactionTime = initial; actor.InCombat = true;
        var x = actor.X; ActorWanderActions.Wander(actor);
        Assert.Equal(expected, actor.ReactionTime); Assert.Equal(x, actor.X); Assert.False(actor.InCombat);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConversationAndStandStillClearCombatBeforeReturning(bool conversation)
    {
        var actor = Room().Actors[0]; actor.InConversation = conversation; actor.StandStill = !conversation;
        actor.ReactionTime = 9; actor.InCombat = true;
        ActorWanderActions.Wander(actor);
        Assert.False(actor.InCombat); Assert.Equal(9, actor.ReactionTime); Assert.Equal(0, actor.MoveCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StraightWalkUsesNativeSpeedAndOptionalDirectionTurn(bool suppressTurn)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ReactionTime = 0; actor.MoveCount = 10;
        actor.MoveDirection = 2; actor.Angle = BamAngle.FromDegrees(10);
        var x = actor.X; var y = actor.Y;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | (suppressTurn ? ActorWanderFlags.NoDirectionTurn : 0));
        Assert.Equal(x, actor.X); Assert.Equal(y.Raw + actor.MovementSpeed.Raw, actor.Y.Raw); Assert.Equal(9, actor.MoveCount);
        Assert.Equal(suppressTurn ? 10 : 45, actor.Angle.ToDegrees(), 5);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState); Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StopIfBlockedControlsRandomDirectionFallback(bool stop)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ReactionTime = 0; actor.OnGround = false; actor.Z = Fixed.FromInt(10);
        actor.MoveCount = 10;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | (stop ? ActorWanderFlags.StopIfBlocked : 0));
        Assert.Equal(stop, sim.CaptureState().NewChaseDirRandomState is null);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
        Assert.Equal(stop ? 9 : 14, actor.MoveCount); Assert.Equal(stop ? 0 : 8, actor.MoveDirection);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedWanderContinuationRestoresMovementAndBothStreams(bool friendly)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ReactionTime = 0; actor.Friendly = friendly;
        var old = SimSavegame.Write(sim, true);
        for (var i = 0; i < 5; i++) ActorWanderActions.Wander(actor);
        var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 30; i++) ActorWanderActions.Wander(actor);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        for (var i = 0; i < 30; i++) ActorWanderActions.Wander(actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old);
        Assert.Equal(0, actor.MoveDirection); Assert.Equal(0, actor.MoveCount);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState); Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void InvalidMovementStateAndRandomFlagsRejectArchives()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.MoveCount = 1;
        var state = sim.CaptureState(); state.Actors[0].MoveDirection = 256;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-wander-header", error);
    }

    [Fact]
    public void LookExWithoutFriendlySeeStateUsesWanderFallback()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Friendly = true; actor.SeeState = -1;
        actor.ReactionTime = 1; actor.InCombat = true; actor.LastHeardTargetId = sim.Players.Single().Id;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck);
        Assert.Equal(0, actor.ReactionTime); Assert.False(actor.InCombat); Assert.Null(actor.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64 }, new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
