namespace HCDE.Playsim.Tests;

public class VmActorRemoveActionsTests
{
    [Fact]
    public void ShouldRemoveWhenPresent_Target() =>
        Assert.True(VmActorRemoveActions.ShouldRemoveWhenPresent(new object()));

    [Fact]
    public void ShouldRemoveWhenPresent_Null() =>
        Assert.False(VmActorRemoveActions.ShouldRemoveWhenPresent(null));

    [Fact]
    public void ShouldRemoveMaster_WhenSet() =>
        Assert.True(VmActorRemoveActions.ShouldRemoveMaster(new object()));

    [Fact]
    public void ShouldRemoveChildInIterator_MatchesHealthGate()
    {
        var master = new object();
        Assert.True(VmActorRemoveActions.ShouldRemoveChildInIterator(master, master, 0, false));
    }

    [Fact]
    public void ShouldRemoveChildInIterator_RemoveAllIgnoresHealth()
    {
        var master = new object();
        Assert.True(VmActorRemoveActions.ShouldRemoveChildInIterator(master, master, 100, removeAll: true));
    }

    [Fact]
    public void RemoveSiblingsLoop_RequiresDistinctActor() =>
        Assert.False(VmActorRemoveActions.RemoveSiblingsLoop(true, true, false, 0, false));

    [Fact]
    public void ShouldRunRemoveSiblingsLoop_WhenMasterSet() =>
        Assert.True(VmActorRemoveActions.ShouldRunRemoveSiblingsLoop(new object()));

    [Fact]
    public void ShouldRemoveReference_WhenNonNull() =>
        Assert.True(VmActorRemoveActions.ShouldRemoveReference("ref"));

    [Fact]
    public void AssignTeleFogTypes_MatchesNativeFields()
    {
        var (src, dst) = VmActorRemoveActions.AssignTeleFogTypes("OldPos", "NewPos");
        Assert.Equal("OldPos", src);
        Assert.Equal("NewPos", dst);
    }

    [Fact]
    public void SwapTeleFogWhenDistinct_SkipsWhenEqual()
    {
        var (src, dst) = VmActorRemoveActions.SwapTeleFogWhenDistinct("Same", "Same");
        Assert.Equal("Same", src);
        Assert.Equal("Same", dst);
        (src, dst) = VmActorRemoveActions.SwapTeleFogWhenDistinct("A", "B");
        Assert.Equal("B", src);
        Assert.Equal("A", dst);
    }

    [Fact]
    public void ShouldSwapTeleFog_WhenTypesDiffer() =>
        Assert.True(VmActorRemoveActions.ShouldSwapTeleFog("A", "B"));

    [Fact]
    public void ShouldRemoveTarget_AndTracer_UsePresenceGate()
    {
        Assert.False(VmActorRemoveActions.ShouldRemoveTarget(null));
        Assert.True(VmActorRemoveActions.ShouldRemoveTracer(new object()));
    }

    [Fact]
    public void ShouldRemoveSiblingCandidate_MatchesNativeGuard()
    {
        var master = new object();
        var self = new object();
        var sibling = new object();
        Assert.True(VmActorRemoveActions.ShouldRemoveSiblingCandidate(master, master, sibling, self, 0, false));
        Assert.False(VmActorRemoveActions.ShouldRemoveSiblingCandidate(master, master, self, self, 0, false));
    }

    [Fact]
    public void ApplySwappedTeleFogFields_RotatesDistinctTypes()
    {
        var (src, dst) = VmActorRemoveActions.ApplySwappedTeleFogFields("Src", "Dst");
        Assert.Equal("Dst", src);
        Assert.Equal("Src", dst);
    }

    [Fact]
    public void ChildHealthAllowsRemove_WhenDeadOrRemoveAll()
    {
        Assert.True(VmActorRemoveActions.ChildHealthAllowsRemove(0, false));
        Assert.False(VmActorRemoveActions.ChildHealthAllowsRemove(50, false));
        Assert.True(VmActorRemoveActions.ChildHealthAllowsRemove(50, true));
    }
}
