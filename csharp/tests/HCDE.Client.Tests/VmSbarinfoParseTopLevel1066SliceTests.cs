namespace HCDE.Client.Tests;

public class VmSbarinfoParseTopLevel1066SliceTests
{
    [Fact]
    public void ResolveLowerHealthCap_PrefersFalseBranchFirst()
    {
        Assert.False(VmSbarinfoParseTopLevel1066Slice.ResolveLowerHealthCap(true, validatedTrueToken: false));
        Assert.True(VmSbarinfoParseTopLevel1066Slice.ResolveLowerHealthCap(false, validatedTrueToken: true));
    }

    [Fact]
    public void ResolveImplicitGameTypeWhenBaseUnset_MatchesNative()
    {
        Assert.Null(VmSbarinfoParseTopLevel1066Slice.ResolveImplicitGameTypeWhenBaseUnset(baseWasSet: true));
        Assert.Equal(0, VmSbarinfoParseTopLevel1066Slice.ResolveImplicitGameTypeWhenBaseUnset(false));
    }

    [Fact]
    public void StatusBarHudPolicy_HandlesAppendVsCreate()
    {
        Assert.Equal(VmSbarinfoParseTopLevel1066Slice.StatusBarHudAction.CreateNewBlock,
            VmSbarinfoParseTopLevel1066Slice.ResolveStatusBarHudAction(isAppendCommand: false, hudIsNull: true));
        Assert.True(VmSbarinfoParseTopLevel1066Slice.ShouldRejectAppendToMissingHud(true, hudIsNull: true));
        Assert.True(VmSbarinfoParseTopLevel1066Slice.ShouldMarkAutomapBar(
            VmSbarinfoMainBlockLoad1064Slice.MatchStatusBarTypeName("automap")));
    }

    [Fact]
    public void ApplyMugShotStateFlag_StacksHealth2Flags()
    {
        var state = VmSbarinfoParseTopLevel1066Slice.ApplyMugShotStateFlag(
            "health2", new VmSbarinfoParseTopLevel1066Slice.MugShotFlagState(false, false, false, false));
        Assert.True(state.UsesLevels);
        Assert.True(state.Health2);
    }

    [Fact]
    public void ClassifyPopupKind_MapsLogKeysStatus()
    {
        Assert.Equal(VmSbarinfoParseTopLevel1066Slice.PopupKind.Keys,
            VmSbarinfoParseTopLevel1066Slice.ClassifyPopupKind("keys"));
    }

    [Fact]
    public void ComputeFadePopupSpeed_UsesTicRate()
    {
        var speed = VmSbarinfoParseTopLevel1066Slice.ComputeFadePopupSpeed(0.5, VmSbarinfoParseTopLevel1066Slice.TicRate);
        Assert.Equal(1.0 / (35 * 0.5), speed, 6);
    }

    [Fact]
    public void BuildFadePopupSpeeds_PairsBothFloats()
    {
        var (s1, s2) = VmSbarinfoParseTopLevel1066Slice.BuildFadePopupSpeeds(1.0, 2.0, 35);
        Assert.True(s1 > s2);
    }

    [Fact]
    public void IsProtrusionFactorAscending_EnforcesStrictIncrease()
    {
        Assert.True(VmSbarinfoParseTopLevel1066Slice.IsProtrusionFactorAscending(
            0.1, VmSbarinfoParseTopLevel1066Slice.ProtrusionInitialLastFactor));
        Assert.False(VmSbarinfoParseTopLevel1066Slice.IsProtrusionFactorAscending(0.1, 0.2));
    }

    [Fact]
    public void ClassifyPopupTransition_RecognizesSlideAndFade()
    {
        Assert.Equal(VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.Fade,
            VmSbarinfoParseTopLevel1066Slice.ClassifyPopupTransition("fade"));
    }

    [Fact]
    public void MergeResolution_ReplacesWhenCommandActive()
    {
        var (w, h) = VmSbarinfoParseTopLevel1066Slice.MergeResolution(1, 2, 640, 480, resolutionCommandActive: true);
        Assert.Equal(640, w);
        Assert.Equal(480, h);
    }
}
