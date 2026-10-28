namespace HCDE.Playsim.Tests;

public class VmPAcsAcsProfile952SliceTests
{
    [Fact]
    public void FormatDumpScriptStatusLine_UsesStateName()
    {
        var line = VmPAcsAcsProfile952Slice.FormatDumpScriptStatusLine(
            "script 1",
            VmPAcsIsClientSideScript.ScriptState.Suspended);
        Assert.Equal("script 1: Suspended", line);
    }

    [Fact]
    public void CreateResetProfileStats_UsesUintMaxMin()
    {
        var stats = VmPAcsAcsProfile952Slice.CreateResetProfileStats();
        Assert.Equal(uint.MaxValue, stats.MinInstrPerRun);
        Assert.Equal(0u, stats.TotalInstr);
    }

    [Fact]
    public void ApplyProfileRun_UpdatesMinMaxAndTotals()
    {
        var stats = VmPAcsAcsProfile952Slice.CreateResetProfileStats();
        stats = VmPAcsAcsProfile952Slice.ApplyProfileRun(stats, 100);
        stats = VmPAcsAcsProfile952Slice.ApplyProfileRun(stats, 50);
        Assert.Equal(150u, stats.TotalInstr);
        Assert.Equal(2u, stats.NumRuns);
        Assert.Equal(50u, stats.MinInstrPerRun);
        Assert.Equal(100u, stats.MaxInstrPerRun);
    }

    [Fact]
    public void ShouldCollectFunctionProfile_OnlyWhenNotImported()
    {
        Assert.True(VmPAcsAcsProfile952Slice.ShouldCollectFunctionProfile(0));
        Assert.False(VmPAcsAcsProfile952Slice.ShouldCollectFunctionProfile(3));
    }

    [Fact]
    public void CompareProfileTotalInstr_SortsDescending()
    {
        var high = new VmPAcsAcsProfile952Slice.AcsProfileStats { TotalInstr = 200 };
        var low = new VmPAcsAcsProfile952Slice.AcsProfileStats { TotalInstr = 10 };
        Assert.True(VmPAcsAcsProfile952Slice.CompareProfileTotalInstr(high, low) < 0);
    }

    [Fact]
    public void AverageInstrPerRun_ZeroWhenNoRuns()
    {
        var stats = VmPAcsAcsProfile952Slice.CreateResetProfileStats();
        Assert.Equal(0u, VmPAcsAcsProfile952Slice.AverageInstrPerRun(stats));
    }

    [Fact]
    public void ResolveProfileDisplayLimit_AllWhenNonPositive()
    {
        Assert.Equal(uint.MaxValue, VmPAcsAcsProfile952Slice.ResolveProfileDisplayLimit(0));
        Assert.Equal(5u, VmPAcsAcsProfile952Slice.ResolveProfileDisplayLimit(5));
    }

    [Fact]
    public void ShouldSkipUnrunProfile_WhenNumRunsZero()
    {
        var stats = VmPAcsAcsProfile952Slice.CreateResetProfileStats();
        Assert.True(VmPAcsAcsProfile952Slice.ShouldSkipUnrunProfile(stats));
    }

    [Fact]
    public void ProfileListingTitle_UsesTopPrefixWhenLimited()
    {
        Assert.Contains("Top 10", VmPAcsAcsProfile952Slice.ProfileListingTitle(false, 10));
    }
}
