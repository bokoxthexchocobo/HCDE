namespace HCDE.Playsim.Tests;

public class VmPLnspecFloorStairsCeilingSliceTests
{
    [Fact]
    public void GenericFloor_RaiseTargetSelectsHighest()
    {
        var kind = (VmPLnspecFloorStairsCeilingSlice.FloorKind)(-1);
        VmPLnspecFloorStairsCeilingSlice.GenericFloor(1, 8, 0, 1, 8, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvDoFloor = (k, _, _, _, _, _, _) => { kind = k; return true; },
        });
        Assert.Equal(VmPLnspecFloorStairsCeilingSlice.FloorKind.RaiseToHighest, kind);
    }

    [Fact]
    public void GenericFloor_CrushFlagSetsTwenty()
    {
        var crush = 0;
        VmPLnspecFloorStairsCeilingSlice.GenericFloor(1, 8, 0, 0, 24, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvDoFloor = (_, _, _, _, c, _, _) => { crush = c; return true; },
        });
        Assert.Equal(20, crush);
    }

    [Fact]
    public void GenericStairs_TogglesOnRepeatableLine()
    {
        var toggled = false;
        Assert.True(VmPLnspecFloorStairsCeilingSlice.GenericStairs(1, 8, 4, 0, 0, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvBuildStairs = (_, _, _, _, _, _, _) => true,
            RepeatSpecial = true,
            LineSpecialId = 100,
            GenericStairsSpecialId = 100,
            ToggleGenericStairsDirection = () => toggled = true,
        }));
        Assert.True(toggled);
    }

    [Fact]
    public void StairsBuildUp_UsesTicsDelay()
    {
        var delay = -1;
        VmPLnspecFloorStairsCeilingSlice.StairsBuildUp(1, 8, 4, 7, 0, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvBuildStairs = (_, _, _, _, d, _, _) => { delay = d; return true; },
        });
        Assert.Equal(VmPLnspecArgMacros.Tics(7), delay);
    }

    [Fact]
    public void CeilingCrushAndRaise_HalvesUpSpeed()
    {
        double up = 0;
        VmPLnspecFloorStairsCeilingSlice.CeilingCrushAndRaise(1, 16, 10, 0, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvDoCeiling = (_, _, down, u, _, _, _, _) => { up = u; return down == 2; },
        });
        Assert.Equal(1, up);
    }

    [Fact]
    public void FloorDonut_ScalesBothSpeeds()
    {
        double pillar = 0, slime = 0;
        VmPLnspecFloorStairsCeilingSlice.FloorDonut(2, 16, 8, new VmPLnspecFloorStairsCeilingSlice.LineContext
        {
            EvDoDonut = (_, p, s) => { pillar = p; slime = s; return true; },
        });
        Assert.Equal(2, pillar);
        Assert.Equal(1, slime);
    }
}
