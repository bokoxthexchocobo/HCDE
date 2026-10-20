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
}
