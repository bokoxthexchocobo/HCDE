using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPMapSlideMoveTests
{
    static (AuthoritySimulation Sim, Actor Mover) OpenRoom()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1, X = 0 }],
        });
        var mover = sim.Players.First();
        mover.SectorIndex = 0;
        return (sim, mover);
    }

    static VmPMapSlideMove.SlideMoveState SlideState(
        AuthoritySimulation sim,
        Actor mover,
        VmPMapSlideMove.SlideActor slide,
        IReadOnlyList<VmPMapSlideMove.SlideLine>? slideLines = null,
        bool blockAllSlideLines = false) =>
        new()
        {
            Sim = sim,
            MapActor = mover,
            Actor = slide,
            TmMove = new VmPMapSlideMove.Vec2(0, 0),
            SlideLines = slideLines ?? Array.Empty<VmPMapSlideMove.SlideLine>(),
            BlockAllSlideLines = blockAllSlideLines,
        };

    [Fact]
    public void HitSlideLine_HorizontalWallClearsYMove()
    {
        var (sim, mover) = OpenRoom();
        var st = SlideState(sim, mover, new VmPMapSlideMove.SlideActor { Z = 0, FloorZ = 0 });
        st.TmMove = new VmPMapSlideMove.Vec2(2, 4);
        VmPMapSlideMove.HitSlideLine(st, new VmPMapSlideMove.SlideLine { Id = "h", DeltaX = 10, DeltaY = 0 });
        Assert.Equal(2, st.TmMove.X);
        Assert.Equal(0, st.TmMove.Y);
    }

    [Fact]
    public void HitSlideLine_VerticalWallClearsXMove()
    {
        var (sim, mover) = OpenRoom();
        var st = SlideState(sim, mover, new VmPMapSlideMove.SlideActor { Z = 0, FloorZ = 0 });
        st.TmMove = new VmPMapSlideMove.Vec2(4, 2);
        VmPMapSlideMove.HitSlideLine(st, new VmPMapSlideMove.SlideLine { Id = "v", DeltaX = 0, DeltaY = 10 });
        Assert.Equal(0, st.TmMove.X);
        Assert.Equal(2, st.TmMove.Y);
    }

    [Fact]
    public void SlideTraverse_PicksBlockingLineWithLowestFrac()
    {
        var (sim, mover) = OpenRoom();
        var line = new VmPMapSlideMove.SlideLine { Id = "block", DeltaX = 1, DeltaY = 0 };
        var st = SlideState(sim, mover, new VmPMapSlideMove.SlideActor { Z = 0, Height = 32, Top = 32, MaxStepHeight = 24 }, [line], blockAllSlideLines: true);
        VmPMapSlideMove.SlideTraverse(st, new VmPMapSlideMove.Vec2(0, 0), new VmPMapSlideMove.Vec2(8, 0));
        Assert.Same(line, st.BestSlideLine);
        Assert.True(st.BestSlideFrac <= 1);
    }

    [Fact]
    public void SlideMove_ReturnsEarlyWhenTryMoveFails()
    {
        var (sim, mover) = OpenRoom();
        var actor = new VmPMapSlideMove.SlideActor { X = 0, Y = 0, VelX = 1, VelY = 0 };
        VmPMapSlideMove.SlideMove(new VmPMapSlideMove.SlideMoveState
        {
            Sim = sim,
            MapActor = mover,
            Actor = actor,
            TryDelta = new VmPMapSlideMove.Vec2(4, 0),
        });
        Assert.Equal(0, actor.X);
    }

    [Fact]
    public void SlideMove_SkipsWhenReactionTime()
    {
        var (sim, mover) = OpenRoom();
        var actor = new VmPMapSlideMove.SlideActor { HasPlayer = true, PlayerIsMo = true, ReactionTime = 5 };
        VmPMapSlideMove.SlideMove(new VmPMapSlideMove.SlideMoveState
        {
            Sim = sim,
            MapActor = mover,
            Actor = actor,
            TryDelta = new VmPMapSlideMove.Vec2(8, 0),
        });
        Assert.Equal(0, actor.X);
    }
}
