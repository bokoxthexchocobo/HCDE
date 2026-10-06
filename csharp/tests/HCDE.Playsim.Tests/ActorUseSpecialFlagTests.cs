using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorUseSpecialFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentUseSpecialClearsLaterFlagAndKeepsArchiveBytes(bool otherOverride)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0);
        if (otherOverride) sim.AddBot(200, 0).UseSpecial = true;
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.ChangeFlag(actor, "USESPECIAL", true);
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.UseSpecial);
        Assert.False(actor.HasUseSpecialOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Null(ActorJumpActions.CheckFlag(actor, "USESPECIAL", 1));
    }

    [Fact]
    public void ExplicitFalseRemainsAnOverrideAfterRestoration()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0); actor.UseSpecial = false;
        var bytes = SimSavegame.Write(sim);
        actor.UseSpecial = true;
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.UseSpecial);
        Assert.True(actor.HasUseSpecialOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData("USESPECIAL", true)]
    [InlineData("USESPECIAL", false)]
    [InlineData("usespecial", true)]
    [InlineData("usespecial", false)]
    [InlineData("Actor.UseSpecial", true)]
    [InlineData("Actor.UseSpecial", false)]
    public void AcsAndFrameFlagActionsShareUseSpecialAndRestore(string flag, bool value)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0);
        var binding = new AcsActivatorBinding { Value = actor };
        var stack = new List<int> { 0, 0, value ? 1 : 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [flag], new AcsGlobalStrings(),
            AcsCallFunctions.SetActorFlag, 3, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
        var check = new List<int> { 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, check, binding, [flag], new AcsGlobalStrings(),
            AcsCallFunctions.CheckFlag, 2, out result));
        Assert.Equal(value ? 1 : 0, result); Assert.Empty(check);
        Assert.Equal(value ? 1 : (int?)null, ActorJumpActions.CheckFlag(actor, flag, 1));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0,
            Action: self => ActorPropertyActions.ChangeFlag(self, flag, !value))], 0);
        actor.States.Enter(actor, 1); Assert.Equal(!value, actor.UseSpecial);
        sim.RestoreState(state); Assert.Equal(value, actor.UseSpecial);
    }
}
