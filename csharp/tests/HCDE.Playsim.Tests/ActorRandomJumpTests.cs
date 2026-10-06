using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRandomJumpTests
{
    [Theory]
    [InlineData(256)]
    [InlineData(int.MaxValue)]
    public void GuaranteedJumpDoesNotConsumeRandomness(int chance)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.Equal(2, ActorJumpActions.Jump(actor, chance, 2)); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(2, ActorJumpActions.Jump(new Actor(), chance, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ImpossibleJumpStillConsumesRandomness(int chance)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.Null(ActorJumpActions.Jump(actor, chance, 2)); Assert.NotEqual(bytes, SimSavegame.Write(sim));
        Assert.NotNull(sim.CaptureState().JumpRandomState);
    }

    [Fact]
    public void RandomJumpReplaysAfterSaveAndKeepsCombatStreamIndependent()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var combat = sim.CombatRandomState;
        ActorJumpActions.Jump(actor, 128, 2); Assert.Equal(combat, sim.CombatRandomState);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var results = Enumerable.Range(0, 100).Select(_ => ActorJumpActions.Jump(actor, 128, 2)).ToArray();
        Assert.Contains(2, results); Assert.Contains(null, results);
        sim.RestoreState(state); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(results, Enumerable.Range(0, 100).Select(_ => ActorJumpActions.Jump(actor, 128, 2)).ToArray());
    }

    [Fact]
    public void ReturnedJumpEntersStateAndRunsItsAction()
    {
        var actor = Room().AddBot(0, 0); var invoked = 0;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, StateAction: self => ActorJumpActions.Jump(self, 256, 2)),
            new(-1, 2, Action: _ => invoked++)], 0);
        actor.States.Enter(actor, 1); Assert.Equal(2, actor.States.Current); Assert.Equal(1, invoked);
    }

    [Theory]
    [InlineData(0, 82)]
    [InlineData(8, 16)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorJumpActions.Jump(sim.AddBot(0, 0), 128, 2); var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12 + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-jumprandom-", error);
    }

    [Fact]
    public void DetachedRandomJumpRequiresSimulation() => Assert.Throws<InvalidOperationException>(() => ActorJumpActions.Jump(new Actor(), 0, 2));

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
