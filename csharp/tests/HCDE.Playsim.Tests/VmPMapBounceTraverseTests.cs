namespace HCDE.Playsim.Tests;

public class VmPMapBounceTraverseTests
{
    [Fact]
    public void BlockEverything_Stops() =>
        Assert.False(VmPMapBounceTraverse.EvaluateTraverseContinue(
            new VmPMapBounceTraverse.BounceTraverseContext(
                true, VmPMapBounceTraverse.MlBlockeverything, true, true, 64, 32, 40, 30, 0, 10, 1, 0.5)));

    [Fact]
    public void OpeningTooLow_Blocks() =>
        Assert.False(VmPMapBounceTraverse.EvaluateTraverseContinue(
            new VmPMapBounceTraverse.BounceTraverseContext(
                true, 0, true, true, 16, 32, 40, 30, 0, 10, 1, 0.5)));

    [Fact]
    public void HorizonDestroys() =>
        Assert.True(VmPMapBounceTraverse.HorizonDestroys(true, true, false));

    [Fact]
    public void BounceWallsRequired() =>
        Assert.False(VmPMapBounceTraverse.BounceWallsEnabled(0));

    [Fact]
    public void BounceWallDestroy_Horizon() =>
        Assert.True(VmPMapBounceTraverse.EvaluateBounceWallDestroy(
            new VmPMapBounceTraverse.BounceWallContext(1, true, 2, true, false, 1, false, 0, false)));

    [Fact]
    public void BounceWallReflect_ShortMove() =>
        Assert.True(VmPMapBounceTraverse.EvaluateBounceWallReflect(
            new VmPMapBounceTraverse.BounceWallContext(1, false, 2, false, false, 2, false, 0.5, true)));
}
