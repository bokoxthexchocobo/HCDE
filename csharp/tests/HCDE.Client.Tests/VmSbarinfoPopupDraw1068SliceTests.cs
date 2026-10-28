namespace HCDE.Client.Tests;

public class VmSbarinfoPopupDraw1068SliceTests
{
    [Fact]
    public void ResolveFadePopupMovingAfterAlphaStep_StopsAtEndpoints()
    {
        Assert.False(VmSbarinfoPopupDraw1068Slice.ResolveFadePopupMovingAfterAlphaStep(1.0, alphaStepApplied: true));
        Assert.True(VmSbarinfoPopupDraw1068Slice.ResolveFadePopupMovingAfterAlphaStep(0.5, true));
    }

    [Fact]
    public void ResolveDefaultTransitionPopupPose_MatchesNativeOpenedClosed()
    {
        var (x, y, moving) = VmSbarinfoPopupDraw1068Slice.ResolveDefaultTransitionPopupPose(false, 200, 320);
        Assert.Equal(200, x);
        Assert.Equal(320, y);
        Assert.False(moving);
    }

    [Fact]
    public void ScalePopupAlphaForDraw_MultipliesMax()
    {
        Assert.Equal(0.5, VmSbarinfoPopupDraw1068Slice.ScalePopupAlphaForDraw(0.25, 2.0));
    }

    [Fact]
    public void ApplyPopupOpenCommand_ArmsMovement()
    {
        var (opened, moving) = VmSbarinfoPopupDraw1068Slice.ApplyPopupOpenCommand();
        Assert.True(opened);
        Assert.True(moving);
    }

    [Fact]
    public void AdjustRelativeCenterX_AppliesHalfWidthWhenRelative()
    {
        Assert.Equal(110.0, VmSbarinfoPopupDraw1068Slice.AdjustRelativeCenterX(true, 10, 200, 1.0));
        Assert.Equal(10.0, VmSbarinfoPopupDraw1068Slice.AdjustRelativeCenterX(false, 10, 200, 1.0));
    }

    [Fact]
    public void BuildPatchNameTableLayout_MatchesNativeOffsets()
    {
        var layout = VmSbarinfoPopupDraw1068Slice.BuildPatchNameTableLayout(5);
        Assert.Equal(5, layout.InvBarOffset);
        Assert.Equal(14, layout.TotalPatchCount);
    }

    [Fact]
    public void ShouldApplyFadeAlphaStep_OnlyWhenFadeTransitionMoving()
    {
        Assert.True(VmSbarinfoPopupDraw1068Slice.ShouldApplyFadeAlphaStep(
            true, VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.Fade));
        Assert.False(VmSbarinfoPopupDraw1068Slice.ShouldApplyFadeAlphaStep(
            true, VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.PushUp));
    }

    [Fact]
    public void NormalizeReadyWeaponAmmo_PromotesSecondaryWhenPrimaryMissing()
    {
        var (primary, secondary) = VmSbarinfoPopupDraw1068Slice.NormalizeReadyWeaponAmmo(null, new object());
        Assert.NotNull(primary);
        Assert.Null(secondary);
    }

    [Fact]
    public void SelectHudIndexForStatusBarDraw_SwitchesToAutomap()
    {
        Assert.Equal(VmSbarinfoPopupDraw1068Slice.AutomapHudBarIndex,
            VmSbarinfoPopupDraw1068Slice.SelectHudIndexForStatusBarDraw(true, true));
        Assert.Equal(VmSbarinfoPopupDraw1068Slice.NormalHudBarIndex,
            VmSbarinfoPopupDraw1068Slice.SelectHudIndexForStatusBarDraw(true, false));
    }

    [Fact]
    public void TickFadePopupAlpha_DelegatesTo1067WhenMoving()
    {
        var alpha = VmSbarinfoPopupDraw1068Slice.TickFadePopupAlpha(
            opened: true, alpha: 0.2, speed: 0.1, speed2: 0.05, movingThisFrame: true);
        Assert.Equal(0.3, alpha, 6);
    }

    [Fact]
    public void ResolveInventoryAmount_ZeroWhenMissing()
    {
        Assert.Equal(0, VmSbarinfoPopupDraw1068Slice.ResolveInventoryAmount(false, 99));
    }
}
