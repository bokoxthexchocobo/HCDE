using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorChangeFlagTests
{
    [Theory]
    [InlineData("SOLID", false)]
    [InlineData("SOLID", true)]
    [InlineData("FLOAT", false)]
    [InlineData("FLOAT", true)]
    [InlineData("NOGRAVITY", false)]
    [InlineData("NOGRAVITY", true)]
    public void AcsMovementChangesSurviveSaveWithBothValues(string flag, bool value)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var stack = new List<int> { 0, 0, value ? 1 : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = actor },
            [flag], new AcsGlobalStrings(), AcsCallFunctions.SetActorFlag, 3, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.ChangeFlag(actor, flag, !value); sim.RestoreState(state);
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var restored)); Assert.Equal(value, restored);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ChangeFlagRunsInFrameAndRejectsUnsupportedNames()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            Action: self => ActorPropertyActions.ChangeFlag(self, "Actor.FLOAT", true))], 0);
        actor.States.Enter(actor, 1); Assert.True(actor.Floating); Assert.False(actor.NoGravity);
        Assert.Throws<ArgumentException>(() => ActorPropertyActions.ChangeFlag(actor, "Unknown", true));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(int.MaxValue)]
    public void InvalidMemoryFlagsRejectWriteAndRestoreBeforeMutatingActor(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors[0].MovementActionFlags = flags; actor.Health = 42;
        var originalHealth = actor.Health;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(originalHealth, actor.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
