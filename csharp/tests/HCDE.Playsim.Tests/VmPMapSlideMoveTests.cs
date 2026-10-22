namespace HCDE.Playsim.Tests;

public class VmPMapSlideMoveTests
{
    [Fact]
    public void HitSlideLine_VerticalWallClearsXMove()
    {
        var st = new VmPMapSlideMove.SlideMoveState
        {
            Actor = new VmPMapSlideMove.SlideActor { Z = 0, FloorZ = 0 },
            TmMove = new VmPMapSlideMove.Vec2(4, 2),
        };
        VmPMapSlideMove.HitSlideLine(st, new VmPMapSlideMove.SlideLine { Id = "v", DeltaX = 0, DeltaY = 10 });
        Assert.Equal(0, st.TmMove.X);
        Assert.Equal(2, st.TmMove.Y);
    }

    [Fact]
    public void SlideTraverse_PicksBlockingLineWithLowestFrac()
    {
        var line = new VmPMapSlideMove.SlideLine { Id = "block", DeltaX = 1, DeltaY = 0 };
        var st = new VmPMapSlideMove.SlideMoveState
        {
            Actor = new VmPMapSlideMove.SlideActor { Z = 0, Height = 32, Top = 32, MaxStepHeight = 24 },
            PathTraverse = (_, _) => new[]
            {
                new VmPMapSlideMove.Intercept { Frac = 0.5, Line = line, HitX = 1, HitY = 0 },
            },
            IsBlockedByLine = (_, _) => true,
        };
        VmPMapSlideMove.SlideTraverse(st, new VmPMapSlideMove.Vec2(0, 0), new VmPMapSlideMove.Vec2(8, 0));
        Assert.Same(line, st.BestSlideLine);
        Assert.Equal(0.5, st.BestSlideFrac);
    }

    [Fact]
    public void SlideMove_SkipsWhenReactionTime()
    {
        var actor = new VmPMapSlideMove.SlideActor { HasPlayer = true, PlayerIsMo = true, ReactionTime = 5 };
        VmPMapSlideMove.SlideMove(new VmPMapSlideMove.SlideMoveState
        {
            Actor = actor,
            TryDelta = new VmPMapSlideMove.Vec2(8, 0),
        });
        Assert.Equal(0, actor.X);
    }
}
