namespace HCDE.Playsim.Tests;

public class VmPSwitchCheckRangeTests
{
    [Fact]
    public void CheckSwitchRange_SkipsWhenNoFlags()
    {
        Assert.True(VmPSwitchCheckRange.CheckSwitchRange(
            new VmPSwitchCheckRange.User(),
            new VmPSwitchCheckRange.Line(),
            0,
            new VmPSwitchCheckRange.CheckContext(),
            false));
    }

    [Fact]
    public void StartButton_RepressSetsTimer()
    {
        var side = new object();
        var list = new List<VmPSwitchCheckRange.ButtonThinker> { new() { Side = side, Timer = 5 } };
        Assert.False(VmPSwitchCheckRange.StartButton(list, side, _ => { }));
        Assert.Equal(1, list[0].Timer);
    }

    [Fact]
    public void EffectiveSwitchFlags_ClearsMidtexWhenNoTexture()
    {
        var line = new VmPSwitchCheckRange.Line { Flags = VmPSwitchCheckRange.Ml3dmidtex };
        Assert.Equal(0, VmPSwitchCheckRange.EffectiveSwitchFlags(line, false));
    }

    [Fact]
    public void NullSide_AllowsSwitch() =>
        Assert.True(VmPSwitchCheckRange.CheckSwitchRange(
            new VmPSwitchCheckRange.User(),
            new VmPSwitchCheckRange.Line { Flags = VmPSwitchCheckRange.MlCheckswitchrange },
            0,
            new VmPSwitchCheckRange.CheckContext(),
            false,
            side: null));

    [Fact]
    public void TopSwitchReachableCompat_UsesInclusiveWhenCompat() =>
        Assert.True(VmPSwitchCheckRange.TopSwitchReachableCompat(
            new VmPSwitchCheckRange.User { Top = 64 },
            64,
            VmPSwitchCheckRange.Compatf2Checkswitchrange));

    [Fact]
    public void RunCheckSwitchRangePipeline_UsesTopSwitchPath()
    {
        var user = new VmPSwitchCheckRange.User { Top = 10, Z = 0 };
        var line = new VmPSwitchCheckRange.Line
        {
            Flags = VmPSwitchCheckRange.MlCheckswitchrange,
            OpenRange = 32,
            TopIsSwitch = true,
        };
        var ctx = new VmPSwitchCheckRange.CheckContext
        {
            TopSwitchReachable = (_, _, _) => true,
        };
        Assert.True(VmPSwitchCheckRange.RunCheckSwitchRangePipeline(user, line, 0, ctx, true));
    }
}
