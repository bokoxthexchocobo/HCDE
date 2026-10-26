namespace HCDE.Playsim.Tests;

public class VmPMapBounceTraverseTests
{
    [Fact]
    public void BlockEverything_Stops() =>
        Assert.False(VmPMapBounceTraverse.EvaluateTraverseContinue(
            new VmPMapBounceTraverse.BounceTraverseContext(
                true, VmPMapBounceTraverse.MlBlockeverything, true, true, false, 64, 32, 40, 30, 0, 10, 1, 0.5)));

    [Fact]
    public void OpeningTooLow_Blocks() =>
        Assert.False(VmPMapBounceTraverse.EvaluateTraverseContinue(
            new VmPMapBounceTraverse.BounceTraverseContext(
                true, 0, true, true, false, 16, 32, 40, 30, 0, 10, 1, 0.5)));

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

    [Fact]
    public void ComputeLeadingCorner_FollowsVelocitySign()
    {
        var (x, y) = VmPMapBounceTraverse.ComputeLeadingCorner(10, 20, 4, 1, -1);
        Assert.Equal(14, x);
        Assert.Equal(16, y);
    }

    [Fact]
    public void RunBounceTraverseLoop_UpdatesBestSlideOnBlock()
    {
        var state = new VmPMapBounceTraverse.BounceTraverseState { ActorHeight = 32, ActorTop = 30, ActorZ = 10 };
        var steps = new[]
        {
            new VmPMapBounceTraverse.TraverseInterceptStep
            {
                IsLine = true,
                LineFlags = VmPMapBounceTraverse.MlBlockeverything,
                Frac = 0.25,
                Line = new object(),
            },
        };
        Assert.False(VmPMapBounceTraverse.RunBounceTraverseLoop(state, steps));
        Assert.Equal(0.25, state.BestSlideFrac);
        Assert.NotNull(state.BestSlideLine);
    }

    [Fact]
    public void RunBounceWallPipeline_DestroyOnHorizon()
    {
        var destroyed = false;
        Assert.True(VmPMapBounceTraverse.RunBounceWallPipeline(new VmPMapBounceTraverse.BounceWallSession
        {
            BounceFlags = VmPMapBounceTraverse.BounceWalls,
            BlockingLine = new object(),
            HorizonLine = true,
            Intercepts = Array.Empty<VmPMapBounceTraverse.TraverseInterceptStep>(),
            DestroyActor = () => destroyed = true,
        }));
        Assert.True(destroyed);
    }
}
