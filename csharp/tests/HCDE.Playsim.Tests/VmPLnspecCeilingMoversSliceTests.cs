namespace HCDE.Playsim.Tests;

public class VmPLnspecCeilingMoversSliceTests
{
    [Fact]
    public void CeilingCrushStop_HexenDefaultRemoves()
    {
        Assert.True(VmPLnspecCeilingMoversSlice.CeilingCrushStop(1, 0, new VmPLnspecCeilingMoversSlice.LineContext
        {
            GameTypeHexen = VmPLnspecPolyobjLineSpecials.GameHexen,
            EvCeilingCrushStop = (_, remove) => remove,
        }));
    }

    [Fact]
    public void CeilingCrushStop_ArgOneKeepsCrusher()
    {
        Assert.False(VmPLnspecCeilingMoversSlice.CeilingCrushStop(1, 1, new VmPLnspecCeilingMoversSlice.LineContext
        {
            EvCeilingCrushStop = (_, remove) => remove,
        }));
    }

    [Fact]
    public void CeilingMoveToValueTimes8_NegativeHeight()
    {
        var height = 0;
        VmPLnspecCeilingMoversSlice.CeilingMoveToValueTimes8(1, 8, 3, true, 0, new VmPLnspecCeilingMoversSlice.LineContext
        {
            EvDoCeiling = c => { height = c.Height; return true; },
        });
        Assert.Equal(-24, height);
    }

    [Fact]
    public void GenericCeiling_RaiseTargetSelectsHighest()
    {
        var op = (VmPLnspecCeilingMoversSlice.CeilingOp)(-1);
        VmPLnspecCeilingMoversSlice.GenericCeiling(1, 8, 4, 1, 8, new VmPLnspecCeilingMoversSlice.LineContext
        {
            EvDoCeiling = c => { op = c.Op; return true; },
        });
        Assert.Equal(VmPLnspecCeilingMoversSlice.CeilingOp.RaiseToHighest, op);
    }

    [Fact]
    public void GenericCeiling_CrushFlagSetsTwenty()
    {
        var crush = 0;
        VmPLnspecCeilingMoversSlice.GenericCeiling(1, 8, 0, 0, 24, new VmPLnspecCeilingMoversSlice.LineContext
        {
            EvDoCeiling = c => { crush = c.Crush; return true; },
        });
        Assert.Equal(20, crush);
    }

    [Fact]
    public void CeilingCrushAndRaiseDist_UsesDistAsHeight()
    {
        var dist = -1;
        VmPLnspecCeilingMoversSlice.CeilingCrushAndRaiseDist(1, 12, 16, 5, 0, new VmPLnspecCeilingMoversSlice.LineContext
        {
            EvDoCeiling = c => { dist = c.Height; return true; },
        });
        Assert.Equal(12, dist);
    }
}
