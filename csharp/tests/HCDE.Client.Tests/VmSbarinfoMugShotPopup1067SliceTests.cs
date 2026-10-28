namespace HCDE.Client.Tests;

public class VmSbarinfoMugShotPopup1067SliceTests
{
    [Fact]
    public void CreateNativeInitDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoMugShotPopup1067Slice.CreateNativeInitDefaults();
        Assert.True(d.LowerHealthCap);
        Assert.Equal(320, d.ResW);
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Center, d.SpacingAlignment);
    }

    [Fact]
    public void ShouldFailMugShotGraphicLength_EnforcesFiveChars()
    {
        Assert.False(VmSbarinfoMugShotPopup1067Slice.ShouldFailMugShotGraphicLength(5));
        Assert.True(VmSbarinfoMugShotPopup1067Slice.ShouldFailMugShotGraphicLength(6));
    }

    [Fact]
    public void ResolveMugShotFrameDelay_HonorsNegativePrefix()
    {
        Assert.Equal(-12, VmSbarinfoMugShotPopup1067Slice.ResolveMugShotFrameDelay(true, 12));
        Assert.Equal(4, VmSbarinfoMugShotPopup1067Slice.ResolveMugShotFrameDelay(false, 4));
    }

    [Fact]
    public void MergeImageIndexAfterLookup_HandlesNullAndDuplicate()
    {
        Assert.Equal(-1, VmSbarinfoMugShotPopup1067Slice.MergeImageIndexAfterLookup(
            VmSbarinfoMugShotPopup1067Slice.ImagePatchKind.NullOrEmpty, false, 0, 5));
        Assert.Equal(2, VmSbarinfoMugShotPopup1067Slice.MergeImageIndexAfterLookup(
            VmSbarinfoMugShotPopup1067Slice.ImagePatchKind.Named, foundExisting: true, 2, 5));
    }

    [Fact]
    public void ResolvePopupInitState_FadeStartsTransparent()
    {
        var (_, _, alpha) = VmSbarinfoMugShotPopup1067Slice.ResolvePopupInitState(
            VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.Fade, 200, 320);
        Assert.Equal(0.0, alpha);
    }

    [Fact]
    public void ComputeSlidePopupYOffset_ClosesWhenOpened()
    {
        var nextY = VmSbarinfoMugShotPopup1067Slice.ComputeSlidePopupYOffset(
            opened: true, height: 100, currentY: 50, ispeed: 8);
        Assert.True(nextY < 50);
    }

    [Fact]
    public void AdvanceFadePopupAlpha_ClampsWhenClosing()
    {
        var alpha = VmSbarinfoMugShotPopup1067Slice.AdvanceFadePopupAlpha(
            opened: false, alpha: 0.2, speed: 0.05, speed2: 0.1);
        Assert.Equal(0.1, alpha, 6);
    }

    [Fact]
    public void ShouldKeepPopupMovingOnSlideAxis_StopsAtEndpoints()
    {
        Assert.False(VmSbarinfoMugShotPopup1067Slice.ShouldKeepPopupMovingOnSlideAxis(0, 100));
        Assert.True(VmSbarinfoMugShotPopup1067Slice.ShouldKeepPopupMovingOnSlideAxis(40, 100));
    }

    [Fact]
    public void ClassifyImagePatch_TreatsNullImageSentinel()
    {
        Assert.Equal(VmSbarinfoMugShotPopup1067Slice.ImagePatchKind.NullOrEmpty,
            VmSbarinfoMugShotPopup1067Slice.ClassifyImagePatch("nullimage"));
    }

    [Fact]
    public void ShouldIterateHudResetSlot_CoversNativeHudCount()
    {
        Assert.True(VmSbarinfoMugShotPopup1067Slice.ShouldIterateHudResetSlot(0, VmSbarinfoMugShotPopup1067Slice.HudSlotCount));
        Assert.False(VmSbarinfoMugShotPopup1067Slice.ShouldIterateHudResetSlot(9, VmSbarinfoMugShotPopup1067Slice.HudSlotCount));
    }
}
