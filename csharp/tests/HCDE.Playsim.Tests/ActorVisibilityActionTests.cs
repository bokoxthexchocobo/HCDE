using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorVisibilityActionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ActionsAndFlagWritesPreserveCollisionAndRoundTrip(bool changeFlag, bool hidden)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); var other = sim.AddBot(200, 0);
        if (changeFlag) ActorPropertyActions.ChangeFlag(other, "INVISIBLE", hidden);
        else if (hidden) ActorPropertyActions.HideThing(other);
        else ActorPropertyActions.UnhideThing(other);
        Assert.True(AcsActorFlags.TryGet(other, "INVISIBLE", out var value)); Assert.Equal(hidden, value);
        Assert.False(ActorPhysics.CanOccupy(sim, actor)); Assert.True(other.Solid); Assert.True(other.Shootable);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        other.Invisible = !hidden; sim.RestoreState(state); Assert.Equal(hidden, other.Invisible);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacyAbsenceClearsHiddenState()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.HideThing(actor); sim.RestoreState(state); Assert.False(actor.Invisible);
    }

    [Fact]
    public void ResurrectionRestoresSupportedClassVisibilityDefault()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004);
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        ActorPropertyActions.HideThing(actor);
        Assert.True(ActorRaiseActions.RaiseSelf(actor)); Assert.False(actor.Invisible);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedVisibilityArchiveIsRejected(bool header)
    {
        var sim = Room(); ActorPropertyActions.UnhideThing(sim.AddBot(200, 0));
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + (header ? 0 : 8)), header ? 89 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
