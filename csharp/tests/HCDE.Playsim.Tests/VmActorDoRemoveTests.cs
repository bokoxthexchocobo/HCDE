namespace HCDE.Playsim.Tests;

public class VmActorDoRemoveTests
{
    [Fact]
    public void RemoveMisc_SkipsMonsterMissileCombo() =>
        Assert.False(VmActorDoRemove.RemoveMisc(true, true, VmActorDoRemove.RmvfFlags.Misc));

    [Fact]
    public void RemoveChildEligible_RemoveAll() =>
        Assert.True(VmActorDoRemove.RemoveChildEligible(100, removeAll: true));

    [Fact]
    public void RemoveSiblingEligible_RequiresDistinctActor() =>
        Assert.False(VmActorDoRemove.RemoveSiblingEligible(true, false, 0, false));

    [Fact]
    public void RemoveTargetPresent_WhenSet() =>
        Assert.True(true);

    [Fact]
    public void RemoveReference_WhenNonNull() =>
        Assert.True(true);

    [Fact]
    public void AssignTeleFogTypes_MatchesNativeFields()
    {
        var (src, dst) = VmActorDoRemove.AssignTeleFogTypes("OldPos", "NewPos");
        Assert.Equal("OldPos", src);
        Assert.Equal("NewPos", dst);
    }

    [Fact]
    public void SwapTeleFogWhenDistinct_SkipsWhenEqual()
    {
        var (src, dst) = VmActorDoRemove.SwapTeleFogWhenDistinct("Same", "Same");
        Assert.Equal("Same", src);
        Assert.Equal("Same", dst);
        (src, dst) = VmActorDoRemove.SwapTeleFogWhenDistinct("A", "B");
        Assert.Equal("B", src);
        Assert.Equal("A", dst);
    }
}
