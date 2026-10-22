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
