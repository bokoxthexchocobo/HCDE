using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorLookBehaviorTests
{
    [Theory]
    [InlineData("LOOKALLAROUND")]
    [InlineData("Actor.lookallaround")]
    [InlineData("STANDSTILL")]
    [InlineData("Actor.standstill")]
    public void LookFlagsUseSharedScriptInterface(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out value)); Assert.False(value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LookAllAroundControlsRearAcquisitionAndExplicitArgumentOverridesIt(bool enabled)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.LookAllAround = enabled;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump);
        Assert.Equal(enabled ? sim.Players.Single().Id : (uint?)null, actor.TargetId);
        actor.SetTargetPointer(null);
        ActorLookActions.LookEx(actor, ActorLookFlags.NoJump, allAround: !enabled);
        Assert.Equal(enabled ? null : (uint?)sim.Players.Single().Id, actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StandStillSuppressesOnlyFriendlyNoiseWander(bool standStill)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Friendly = true; actor.StandStill = standStill;
        actor.LastHeardTargetId = sim.Players.Single().Id;
        ActorLookActions.LookEx(actor, ActorLookFlags.NoSightCheck | ActorLookFlags.NoJump);
        Assert.Equal(standStill ? 0 : 1, actor.States.Current);
        Assert.Null(actor.TargetId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FlagsAndChecksumSurviveSaveAndOlderSaveResetsBoth(int flags)
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.LookAllAround = (flags & 1) != 0; actor.StandStill = (flags & 2) != 0;
        sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum; Assert.NotEqual(original, hash);
        var saved = SimSavegame.Write(sim, true);
        actor.LookAllAround = actor.StandStill = false; SimSavegame.Apply(sim, saved);
        Assert.Equal((flags & 1) != 0, actor.LookAllAround); Assert.Equal((flags & 2) != 0, actor.StandStill);
        Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(actor.LookAllAround); Assert.False(actor.StandStill);
        Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void InvalidFlagBitsRejectStructuredAndBinaryArchives()
    {
        var sim = Room(); sim.Actors[0].StandStill = true;
        var state = sim.CaptureState(); state.Actors[0].LookBehaviorFlags = 4;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-lookbehavior-state", error);
    }

    [Fact]
    public void RevivalRestoresRepresentedDefaults()
    {
        var actor = Room().Actors[0]; actor.LookAllAround = actor.StandStill = true;
        ActorRaise.ReviveSupported(actor);
        Assert.False(actor.LookAllAround); Assert.False(actor.StandStill);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = -128 }]
        });
        var actor = sim.Actors[0]; actor.SeeState = 1;
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1)], 0);
        sim.RestoreState(sim.CaptureState());
        return sim;
    }
}
