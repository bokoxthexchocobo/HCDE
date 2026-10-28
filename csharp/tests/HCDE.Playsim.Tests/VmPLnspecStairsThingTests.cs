namespace HCDE.Playsim.Tests;

public class VmPLnspecStairsThingTests
{
    [Fact]
    public void StairsBuildUp_UsesTicsDelay()
    {
        var invoke = VmPLnspecStairsLineSpecials.BuildUp(1, 8, 4, 7, 0);
        Assert.Equal(30, invoke.DelayTics);
    }

    [Fact]
    public void GenericStairs_TogglesDirectionBit()
    {
        Assert.Equal(0, VmPLnspecStairsLineSpecials.ToggleGenericStairsDirection(1));
    }

    [Fact]
    public void ThingStop_ZeroesVelocityWhenActorPresent()
    {
        Assert.True(VmPLnspecThingRaiseStop.ShouldZeroVelocity(true));
    }

    [Fact]
    public void ThingSetGoal_ClearsChaseGoalWhenArgZero()
    {
        Assert.Equal(0, VmPLnspecThingRaiseStop.ApplyChaseGoalFlag(VmPLnspecThingRaiseStop.Mf5ChaseGoal, 0));
    }
}
