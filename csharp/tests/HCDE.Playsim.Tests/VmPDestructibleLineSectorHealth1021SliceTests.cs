namespace HCDE.Playsim.Tests;

public class VmPDestructibleLineSectorHealth1021SliceTests
{
    [Fact]
    public void ResolveLineGetHealth_PrefersGroupWhenLinked()
    {
        Assert.Equal(40, VmPDestructibleLineSectorHealth1021Slice.ResolveLineGetHealth(10, 2, 40));
        Assert.Equal(10, VmPDestructibleLineSectorHealth1021Slice.ResolveLineGetHealth(10, 0, 40));
    }

    [Fact]
    public void ApplyLineSetHealth_ClampsAndFlagsGroup()
    {
        var (health, propagate) = VmPDestructibleLineSectorHealth1021Slice.ApplyLineSetHealth(5, -3, 7);
        Assert.Equal(0, health);
        Assert.True(propagate);
        (_, propagate) = VmPDestructibleLineSectorHealth1021Slice.ApplyLineSetHealth(5, 12, 0);
        Assert.False(propagate);
    }

    [Fact]
    public void ResolveSectorGetHealth_MatchesNativeSwitch()
    {
        Assert.Equal(11, VmPDestructibleLineSectorHealth1021Slice.ResolveSectorGetHealth(
            VmPDestructibleLineSectorHealth1021Slice.SecPartFloor,
            floorHealth: 1, ceilingHealth: 2, health3D: 3,
            floorGroupId: 5, ceilingGroupId: 0, health3DGroupId: 0,
            floorGroupHealth: 11, ceilingGroupHealth: null, health3DGroupHealth: null));
        Assert.Equal(0, VmPDestructibleLineSectorHealth1021Slice.ResolveSectorGetHealth(
            99, 1, 2, 3, 0, 0, 0, null, null, null));
    }

    [Fact]
    public void RouteSectorSetHealth_SelectsGroupPerPart()
    {
        var floor = VmPDestructibleLineSectorHealth1021Slice.RouteSectorSetHealth(
            VmPDestructibleLineSectorHealth1021Slice.SecPartFloor, 8, 1, 2, 3);
        Assert.True(floor.ValidPart);
        Assert.Equal(1, floor.GroupId);
        Assert.Equal(8, floor.ClampedHealth);

        var invalid = VmPDestructibleLineSectorHealth1021Slice.RouteSectorSetHealth(5, 10, 0, 0, 0);
        Assert.False(invalid.ValidPart);
    }

    [Fact]
    public void ShouldPropagateGroupHealth_WhenGroupNonZero()
    {
        Assert.True(VmPDestructibleLineSectorHealth1021Slice.ShouldPropagateGroupHealth(3));
        Assert.False(VmPDestructibleLineSectorHealth1021Slice.ShouldPropagateGroupHealth(0));
    }

    [Fact]
    public void BuildLineSetHealthMutation_MatchesNativeGroupPath()
    {
        var grp = new VmPDestructibleHealthGroups.HealthGroup { Id = 2, Health = 99 };
        var mut = VmPDestructibleLineSectorHealth1021Slice.BuildLineSetHealthMutation(
            -1, 2, id => id == 2 ? grp : null);
        Assert.Equal(0, mut.LineHealth);
        Assert.True(mut.ApplyGroup);
        Assert.Equal(0, mut.GroupHealth);
    }

    [Fact]
    public void BuildSectorSetHealthMutation_InvalidPartNotHandled()
    {
        var mut = VmPDestructibleLineSectorHealth1021Slice.BuildSectorSetHealthMutation(
            8, 10, 0, 0, 0, _ => null);
        Assert.False(mut.Handled);
    }

    [Fact]
    public void ExecuteSectorGetHealth_UsesLookupPerPart()
    {
        int? Lookup(int id) => id == 3 ? 77 : null;
        var v = VmPDestructibleLineSectorHealth1021Slice.ExecuteSectorGetHealth(
            VmPDestructibleLineSectorHealth1021Slice.SecPart3D,
            1, 2, 5, 0, 0, 3, Lookup);
        Assert.Equal(77, v);
    }
}
