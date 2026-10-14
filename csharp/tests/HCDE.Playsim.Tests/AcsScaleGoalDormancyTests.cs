using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScaleGoalDormancyTests
{
    [Theory]
    [InlineData(29, 98304, 98304)]
    [InlineData(30, -32768, -32768)]
    [InlineData(29, 0, 0)]
    [InlineData(30, 16777217, 16777216)]
    [InlineData(29, 16777219, 16777220)]
    public void ScaleAssignmentUsesNativeFloatPrecisionAndReplays(int property, int input, int expected)
    {
        var sim = Room(); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        AcsActorProperties.Set(sim, actor, 0, property, input);
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, property));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, property, expected));
        Assert.Equal(1, property == 29 ? actor.ScaleY : actor.ScaleX);
        var saved = SimSavegame.Write(sim, true);
        actor.ScaleX = actor.ScaleY = 3;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, property));
        SimSavegame.Apply(sim, old); Assert.Equal(1, actor.ScaleX); Assert.Equal(1, actor.ScaleY);
    }

    [Theory]
    [InlineData(29, .5, 0)]
    [InlineData(29, 1.5, 2)]
    [InlineData(30, -1.5, -2)]
    [InlineData(30, -2.5, -2)]
    public void ScaleQueryUsesNativeHalfEvenFixedRounding(int property, double fractionalRaw, int expected)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        if (property == 29) actor.ScaleX = fractionalRaw / 65536; else actor.ScaleY = fractionalRaw / 65536;
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, property));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    public void ChaseGoalSharesScriptFlagAndExistingSavedState(int value)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        AcsActorProperties.Set(sim, null, 7, 13, value);
        Assert.Equal(value != 0, actor.ChaseGoal);
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 13, value));
        Assert.True(AcsActorFlags.TryGet(actor, "CHASEGOAL", out var flag)); Assert.Equal(value != 0, flag);
        var saved = SimSavegame.Write(sim, true); actor.ChaseGoal = value == 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(value != 0, actor.ChaseGoal);
        Assert.True(AcsActorFlags.TrySet(actor, "CHASEGOAL", false));
        Assert.Equal(0, AcsActorProperties.Get(sim, actor, 0, 13));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DormancyCanBeQueriedButAcsAssignmentDoesNotActivateOrDeactivate(bool dormant)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        if (dormant) actor.Deactivate(null);
        var tics = actor.States.RemainingTics;
        Assert.Equal(dormant ? 1 : 0, AcsActorProperties.Get(sim, actor, 0, 31));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 31, dormant ? -9 : 0));
        AcsActorProperties.Set(sim, actor, 0, 31, dormant ? 0 : 1);
        Assert.Equal(dormant, actor.Dormant); Assert.Equal(tics, actor.States.RemainingTics);
        var saved = SimSavegame.Write(sim, true);
        if (dormant) actor.Activate(null); else actor.Deactivate(null);
        SimSavegame.Apply(sim, saved); Assert.Equal(dormant, actor.Dormant);
    }

    [Fact]
    public void MissingAndDestroyedActorsDoNotMatchZeroProperties()
    {
        var sim = Room(); var actor = sim.Actors.Single();
        foreach (var property in new[] { 13, 29, 30, 31 })
            Assert.False(AcsActorProperties.Check(sim, null, 999, property, 0));
        actor.Destroy();
        foreach (var property in new[] { 13, 29, 30, 31 })
        {
            AcsActorProperties.Set(sim, actor, 0, property, 7);
            Assert.Equal(0, AcsActorProperties.Get(sim, actor, 0, property));
            Assert.False(AcsActorProperties.Check(sim, actor, 0, property, 0));
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Id = 7 }]
    });
}
