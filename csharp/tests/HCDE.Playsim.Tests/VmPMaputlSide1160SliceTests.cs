namespace HCDE.Playsim.Tests;

public class VmPMaputlSide1160SliceTests
{
    [Fact]
    public void Search_BuildsTheFrontLineAndReturnsTheHostActor()
    {
        var none = VmPMaputlSide1160Slice.RoughSearch(false, 4, 5, 0, 12, true, true, 45, 9);
        Assert.Equal(-1, none.FoundId);
        Assert.False(none.Front);

        var ahead = VmPMaputlSide1160Slice.RoughSearch(true, 4, 5, 0, 12, true, true, 45, 9);
        Assert.Equal(9, ahead.FoundId);
        Assert.True(ahead.Front);
        Assert.Equal(4, ahead.FrontX);
        Assert.Equal(5, ahead.FrontY);
        Assert.Equal(0, ahead.FrontDx);
        Assert.Equal(-1, ahead.FrontDy);
        Assert.True(ahead.OnlySeekable);
        Assert.Equal(45, ahead.Fov);
        Assert.Equal(12, ahead.Distance);

        var open = VmPMaputlSide1160Slice.RoughSearch(true, 4, 5, 90, 3, false, false, 0, -1);
        Assert.Equal(-1, open.FoundId);
        Assert.False(open.Front);
        Assert.Equal(0, open.FrontDx);
        Assert.Equal(0, open.FrontDy);
    }

    [Fact]
    public void SlowAndVanilla_SplitOnTheLineAndKeepTheSignShortcut()
    {
        Assert.Equal(1, VmPMaputlSide1160Slice.SlowSide(1d / 65536d, 1d / 65536d, 0, 0, 1, 1));
        Assert.Equal(1, VmPMaputlSide1160Slice.SlowSide(-1d / 65536d, 1d / 65536d, 0, 0, 1, 1));
        Assert.Equal(1, VmPMaputlSide1160Slice.SlowSide(1, 1, 0, 0, 65536, 65536));
        Assert.Equal(0, VmPMaputlSide1160Slice.PreciseSide(65536, 65536, 0, 0, 65536, 65536));
        Assert.Equal(0, VmPMaputlSide1160Slice.SlowSide(1, 0, 0, 0, 65536, 65536));
        Assert.Equal(1, VmPMaputlSide1160Slice.SlowSide(-1, 0, 0, 0, 0, 65536));
        Assert.Equal(0, VmPMaputlSide1160Slice.SlowSide(1, 0, 0, 0, 0, 65536));

        Assert.Equal(1, VmPMaputlSide1160Slice.VanillaSide(-1, 0, 0, 0, 0, 10));
        Assert.Equal(0, VmPMaputlSide1160Slice.VanillaSide(1, 0, 0, 0, 0, 10));
        Assert.Equal(0, VmPMaputlSide1160Slice.VanillaSide(0, 0, 0, 0, 10, 0));
        Assert.Equal(1, VmPMaputlSide1160Slice.VanillaSide(0, 1, 0, 0, 10, 0));
        Assert.Equal(1, VmPMaputlSide1160Slice.VanillaSide(1, 1, 0, 0, 2, 2));
        Assert.Equal(0, VmPMaputlSide1160Slice.PointSide(1, 1, 0, 0, 2, 2, true, true));
        Assert.Equal(1, VmPMaputlSide1160Slice.PointSide(1, 1, 0, 0, 2, 2, true, false));
        Assert.Equal(0, VmPMaputlSide1160Slice.PointSide(1, 1, 0, 0, 2, 2, false, false));
        Assert.Equal(1, VmPMaputlSide1160Slice.VanillaSide(1, 0, 0, 0, -2, -2));
    }

    [Fact]
    public void Walk_UsesPreciseForGameNodesAndSlowForTheBuggySector()
    {
        var split = new[]
        {
            new VmPMaputlSide1160Slice.Node(0, 0, 65536, 65536, Sub(2), Sub(3)),
        };
        var sectors = new[] { 7, 0, 20, 30 };
        Assert.Equal(0, VmPMaputlSide1160Slice.Subsector(1, 1, Array.Empty<VmPMaputlSide1160Slice.Node>()));
        Assert.Equal(4, VmPMaputlSide1160Slice.SectorBuggy(1, 1, 4, Array.Empty<VmPMaputlSide1160Slice.Node>(), sectors));
        Assert.Equal(2, VmPMaputlSide1160Slice.Subsector(1, 1, split));
        Assert.Equal(30, VmPMaputlSide1160Slice.SectorBuggy(1, 1, 4, split, sectors));
        Assert.Equal(0, VmPMaputlSide1160Slice.RenderSubsector(65536, 65536, Array.Empty<VmPMaputlSide1160Slice.Node>()));
        Assert.Equal(2, VmPMaputlSide1160Slice.RenderSubsector(65536, 65536, split));

        var tree = new[]
        {
            new VmPMaputlSide1160Slice.Node(0, 0, 0, 65536, Next(1), Sub(4)),
            new VmPMaputlSide1160Slice.Node(0, 0, 65536, 0, Sub(5), Sub(6)),
        };
        Assert.Equal(5, VmPMaputlSide1160Slice.Subsector(1, -1, tree));
        Assert.Equal(50, VmPMaputlSide1160Slice.SectorBuggy(1, -1, 4, tree, new[] { 0, 0, 0, 0, 40, 50, 60 }));
    }

    [Fact]
    public void Box_CrossesAVerticalAndUsesCompatOnlyWhenImprecise()
    {
        Assert.Equal(-1, VmPMaputlSide1160Slice.BoxSide(-1, -1, 1, 1, 0, 0, 0, 1, false));
        Assert.Equal(0, VmPMaputlSide1160Slice.BoxSide(4, -1, 6, 1, 0, 0, 0, 1, false));
        Assert.Equal(1, VmPMaputlSide1160Slice.BoxSide(-6, -1, -4, 1, 0, 0, 0, 1, false));
        Assert.Equal(0, VmPMaputlSide1160Slice.BoxSide(-6, -1, -4, 1, 0, 0, 0, -1, false));
        Assert.Equal(-1, VmPMaputlSide1160Slice.BoxSide(-1, -1, 1, 1, 0, 0, 1, 0, false));
        Assert.Equal(-1, VmPMaputlSide1160Slice.BoxSide(-2, -2, 2, 2, 0, 0, 1, 1, false));
        Assert.Equal(0, VmPMaputlSide1160Slice.ActorSide(10, -10, 1, 0, 0, 1, 1, false));
        Assert.Equal(-1, VmPMaputlSide1160Slice.ActorSide(0, 0, 1, 0, 0, 0, 1, false));
        Assert.Equal(1, VmPMaputlSide1160Slice.ActorSide(0, 0, 0, 5, 0, 0, 1, false));
    }

    private static int Sub(int id)
    {
        return (id << 1) | 1;
    }

    private static int Next(int id)
    {
        return id << 1;
    }
}
