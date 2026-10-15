using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorConversationTests
{
    [Theory]
    [InlineData("INCONVERSATION")]
    [InlineData("Actor.inconversation")]
    public void FlagSupportsSharedScriptAccess(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var set)); Assert.True(set);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.InConversation);
    }

    [Fact]
    public void LookExConversationGuardPrecedesGoalAndThresholdChanges()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.InConversation = true;
        actor.Special = 229; actor.SpecialArgs[2] = 5; actor.ReactionTime = 17;
        actor.Brain!.SetChaseThreshold(40, false);
        ActorLookActions.LookEx(actor);
        Assert.Equal(229, actor.Special); Assert.Equal(17, actor.ReactionTime);
        Assert.Equal(40, actor.Brain.Threshold); Assert.Null(actor.TargetId);
        Assert.Equal(0, actor.States.Current); Assert.Empty(sim.DrainSoundRequests());
        actor.InConversation = false; ActorLookActions.LookEx(actor);
        Assert.Equal(0, actor.Special); Assert.Equal(0, actor.Brain.Threshold);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void ChaseConversationGuardLeavesMotionAndReactionUntouched()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.InConversation = true;
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 20;
        actor.VelocityX = Fixed.FromInt(3); actor.Brain!.SetChaseThreshold(40, false);
        actor.Brain.Tick(sim, actor);
        Assert.Equal(20, actor.ReactionTime); Assert.Equal(40, actor.Brain.Threshold);
        Assert.Equal(Fixed.FromInt(3), actor.VelocityX);
        actor.InConversation = false; actor.Brain.Tick(sim, actor);
        Assert.Equal(19, actor.ReactionTime); Assert.Equal(39, actor.Brain.Threshold);
    }

    [Fact]
    public void NoiseRemainsRememberedWithoutWakingUntilConversationEnds()
    {
        var sim = Room(); var actor = sim.Actors[0]; var player = sim.Players.Single(); actor.InConversation = true;
        SoundPropagation.Alert(sim, player);
        Assert.Equal(player.Id, actor.LastHeardTargetId); Assert.Null(actor.TargetId); Assert.Equal(0, actor.States.Current);
        actor.InConversation = false;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoJump);
        Assert.Equal(player.Id, actor.TargetId);
    }

    [Fact]
    public void ConversationFlagAndChecksumSurviveSaveAndOldSaveResets()
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.InConversation = true; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.InConversation = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.InConversation); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(actor.InConversation); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void InvalidConversationFlagRejectsStructuredAndBinaryData()
    {
        var sim = Room(); sim.Actors[0].InConversation = true;
        var state = sim.CaptureState(); state.Actors[0].InConversationFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-conversation-state", error);
    }

    [Fact]
    public void RevivalRestoresDefaultConversationFlag()
    {
        var actor = Room().Actors[0]; actor.InConversation = true;
        ActorRaise.ReviveSupported(actor); Assert.False(actor.InConversation);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 128 }]
        });
        var actor = sim.Actors[0]; actor.SeeState = 1;
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1)], 0);
        sim.RestoreState(sim.CaptureState());
        return sim;
    }
}
