namespace HCDE.Playsim.Tests;

public class VmPMapAimTraverseMainTests
{
    [Fact]
    public void StopWhenBetterFound_SkipsFartherFrac() =>
        Assert.True(VmPMapAimTraverseMain.StopWhenBetterFound(0.9, 0.5));

    [Fact]
    public void LinePortalEntry_FrontOnly() =>
        Assert.True(VmPMapAimTraverseMain.LinePortalEntry(true, 0));

    [Fact]
    public void LineBlocksAim_OneSided() =>
        Assert.True(VmPMapAimTraverseMain.LineBlocksAim(0));

    [Fact]
    public void OpeningBlocks_ClosedRange() =>
        Assert.True(VmPMapAimTraverseMain.OpeningBlocks(0, 10, 5));

    [Fact]
    public void LineIntercept_StopsOnBlock() =>
        Assert.True(VmPMapAimTraverseMain.EvaluateLineIntercept(
            true, false, 1, 0, 8, 0, 16, 32, 0.5, 100, 0, 0, true, 0).StopTraverse);

    [Fact]
    public void LineIntercept_EntersPortal() =>
        Assert.True(VmPMapAimTraverseMain.EvaluateLineIntercept(
            true, true, 0, VmPMapAimTraverseMain.MlTwosided, 8, 0, 16, 32, 0.5, 100, 0, 0, true, 0).EnterLinePortal);

    [Fact]
    public void SkipThingForAim_NonShootable() =>
        Assert.True(VmPMapAimTraverseMain.SkipThingForAim(
            new VmPMapAimTraverseMain.ThingFilterContext(0, false, false, false, false, false, true, false)));

    [Fact]
    public void SkipThingForAim_GhostThruWeapon() =>
        Assert.True(VmPMapAimTraverseMain.SkipThingForAim(
            new VmPMapAimTraverseMain.ThingFilterContext(0, false, true, true, true, true, true, false)));

    [Fact]
    public void FfSightNarrow_StopWhenRangeCloses() =>
        Assert.True(VmPMapAimTraverseMain.EvaluateFfSightNarrow(
            new VmPMapAimTraverseMain.FfSightNarrowContext(true, false, false, 0.5, 0.4, 0.6, 0.2)).ShouldStop);

    [Fact]
    public void Check3dRangeExceeded_WhenBeyondAttackRange() =>
        Assert.True(VmPMapAimTraverseMain.Check3dRangeExceeded(
            VmPMapAimTraverseMain.AlfCheck3d, 0.5, 200, 0.75, 100));

    [Fact]
    public void SmartAim_RecordsFriend() =>
        Assert.Equal(
            VmPMapAimTraverseMain.SmartAimDecision.RecordFriend,
            VmPMapAimTraverseMain.EvaluateSmartAim(
                new VmPMapAimTraverseMain.SmartAimContext(1, 0, null, true, false, false)));

    [Fact]
    public void SmartAim_HitsMonster() =>
        Assert.Equal(
            VmPMapAimTraverseMain.SmartAimDecision.HitLineTarget,
            VmPMapAimTraverseMain.EvaluateSmartAim(
                new VmPMapAimTraverseMain.SmartAimContext(2, 0, null, false, true, false)));

    [Fact]
    public void ExitPointClearsTopShot() =>
        Assert.True(VmPMapAimTraverseMain.ExitPointClearsTopShot(0.5, 50, 64, 80, 0.5));

    [Fact]
    public void MidThingPitch_Averages() =>
        Assert.Equal(0.75, VmPMapAimTraverseMain.MidThingPitch(1.0, 0.5));
}
