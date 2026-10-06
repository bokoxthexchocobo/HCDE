using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorLinkFlagTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void LinkFlagAffectsCollisionAndRoundTrips(int value, bool excluded)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); var blocker = sim.AddBot(200, 0);
        ActorPropertyActions.ChangeLinkFlags(blocker, value);
        Assert.Equal(excluded, blocker.NoBlockmap); Assert.Equal(!excluded, blocker.IsBlockmapActor);
        Assert.Equal(excluded, ActorPhysics.CanOccupy(sim, actor));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        blocker.NoBlockmap = !excluded; sim.RestoreState(state);
        Assert.Equal(excluded, blocker.NoBlockmap); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void SentinelPreservesStateAndLegacyAbsenceRestoresDefault()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.ChangeLinkFlags(actor);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.NoBlockmap = true; sim.RestoreState(state); Assert.False(actor.NoBlockmap);
    }

    [Fact]
    public void ChangeFlagExplicitClearAtDefaultSurvivesRestore()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        ActorPropertyActions.ChangeFlag(actor, "NOBLOCKMAP", false);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.NoBlockmap = true; sim.RestoreState(state); Assert.False(actor.NoBlockmap);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnsupportedSectorChangeFailsBeforeBlockmapWrite()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        Assert.Throws<NotSupportedException>(() => ActorPropertyActions.ChangeLinkFlags(actor, 1, 0));
        Assert.False(actor.NoBlockmap); Assert.False(actor.HasBlockmapOverride);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedBlockmapTrailerIsRejected(bool header)
    {
        var sim = Room(); ActorPropertyActions.ChangeLinkFlags(sim.AddBot(200, 0), 1);
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + (header ? 0 : 8)), header ? 87 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
