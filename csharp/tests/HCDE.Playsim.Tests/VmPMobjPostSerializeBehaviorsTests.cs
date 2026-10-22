namespace HCDE.Playsim.Tests;

public class VmPMobjPostSerializeBehaviorsTests
{
    [Fact]
    public void PostSerialize_ClearsTouchLists()
    {
        var cleared = 0;
        VmPMobjPostSerializeBehaviors.PostSerializeClearTouchLists(() => cleared++, () => cleared++, () => { }, () => { });
        Assert.Equal(2, cleared);
    }

    [Fact]
    public void ShouldRestorePlayerSkin_WhenEligible() =>
        Assert.True(VmPMobjPostSerializeBehaviors.ShouldRestorePlayerSkin(true, true, 0, true));

    [Fact]
    public void RemoveBehaviorFound_Destroys()
    {
        var destroyed = false;
        Assert.True(VmPMobjPostSerializeBehaviors.RemoveBehaviorFound(new object(), () => destroyed = true));
        Assert.True(destroyed);
    }

    [Fact]
    public void AddBehaviorAbstract_RejectsAbstract() =>
        Assert.False(VmPMobjPostSerializeBehaviors.AddBehaviorAbstract(true, true));

    [Fact]
    public void TickBehaviorsQueuesNull_AddsKey()
    {
        var list = new List<string>();
        Assert.True(VmPMobjPostSerializeBehaviors.TickBehaviorsQueuesNull(null, list, "x"));
        Assert.Contains("x", list);
    }
}
