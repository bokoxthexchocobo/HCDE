namespace HCDE.Playsim.Tests;

public class VmPSightBlock1163SliceTests
{
    [Fact]
    public void Blocks_StopsOneSidedAndLetsAnImpactScriptThrough()
    {
        var check = new VmPSightBlock1163Slice.Check();
        var oneSided = Line(1, false, 0, 0, 0, 0);
        Assert.True(VmPSightBlock1163Slice.Blocks(check, oneSided));

        var open = Line(2, true, VmPSightBlock1163Slice.TwoSided, 0, 0, 0);
        Assert.False(VmPSightBlock1163Slice.Blocks(check, open));

        var blind = Line(3, true, VmPSightBlock1163Slice.TwoSided | VmPSightBlock1163Slice.BlockSight, 0, 0, 0);
        Assert.True(VmPSightBlock1163Slice.Blocks(check, blind));

        var wall = Line(4, true, VmPSightBlock1163Slice.TwoSided | VmPSightBlock1163Slice.BlockEverything, 0, 0, 0);
        Assert.True(VmPSightBlock1163Slice.Blocks(check, wall));

        check.Flags = VmPSightBlock1163Slice.SeePastBlockEverything;
        wall.Activation = 0;
        Assert.True(VmPSightBlock1163Slice.Blocks(check, wall));

        check.Flags |= VmPSightBlock1163Slice.SeePastShootable;
        Assert.False(VmPSightBlock1163Slice.Blocks(check, wall));

        check.Flags = VmPSightBlock1163Slice.SeePastBlockEverything;
        wall.Activation = VmPSightBlock1163Slice.Impact;
        wall.Special = 1;
        Assert.True(VmPSightBlock1163Slice.Blocks(check, wall));

        wall.Special = VmPSightBlock1163Slice.AcsExecute;
        wall.Arg1 = 0;
        check.LevelNum = 3;
        Assert.False(VmPSightBlock1163Slice.Blocks(check, wall));

        wall.Special = VmPSightBlock1163Slice.AcsExecuteAlways;
        wall.Arg1 = 5;
        check.LevelNum = 5;
        Assert.False(VmPSightBlock1163Slice.Blocks(check, wall));

        wall.Arg1 = 5;
        check.LevelNum = 3;
        Assert.True(VmPSightBlock1163Slice.Blocks(check, wall));
    }

    [Fact]
    public void CheckLine_StoresACrossingAndSkipsARepeat()
    {
        var check = Eye();
        check.ValidCount = 4;
        var missed = Line(9, true, VmPSightBlock1163Slice.TwoSided, 0, 1, 0);
        missed.Dx = 10;
        Assert.True(VmPSightBlock1163Slice.CheckLine(check, missed));
        Assert.Empty(check.Intercepts);
        Assert.Equal(4, missed.ValidCount);

        check.Trace = new VmPSightBlock1163Slice.Divline(0, 0, 1, 0);
        var far = Vertical(8, 5);
        Assert.True(VmPSightBlock1163Slice.CheckLine(check, far));
        Assert.Empty(check.Intercepts);

        check.Trace = new VmPSightBlock1163Slice.Divline(0, 0, 10, 0);
        var hit = Vertical(1, 5);
        Assert.True(VmPSightBlock1163Slice.CheckLine(check, hit));
        Assert.Single(check.Intercepts);
        Assert.True(check.Intercepts[0].IsALine);
        Assert.Equal(1, check.Intercepts[0].Line!.Id);
        Assert.Equal(1, check.Counts[3]);
        Assert.True(VmPSightBlock1163Slice.CheckLine(check, hit));
        Assert.Single(check.Intercepts);
        Assert.Equal(1, check.Counts[3]);

        var blocked = Vertical(2, 5);
        blocked.HasBack = false;
        blocked.Flags = 0;
        Assert.False(VmPSightBlock1163Slice.CheckLine(check, blocked));
        Assert.Single(check.Intercepts);

        check.PortalFound = true;
        var kept = Vertical(3, 6);
        kept.HasBack = false;
        kept.Flags = 0;
        Assert.True(VmPSightBlock1163Slice.CheckLine(check, kept));
        Assert.Equal(2, check.Intercepts.Count);
        Assert.Equal(2, check.Counts[3]);
    }

    [Fact]
    public void Block_StopsWithoutAPortalAndKeepsGoingThroughOne()
    {
        var stop = Eye();
        stop.ValidCount = 7;
        var later = Vertical(6, 8);
        int code = VmPSightBlock1163Slice.BlockLines(
            stop, 2, 1, 3, false, false,
            Map<VmPSightBlock1163Slice.PolyLink?>(6, null),
            Map<IReadOnlyList<VmPSightBlock1163Slice.Line>>(6, new List<VmPSightBlock1163Slice.Line>
            {
                OneSided(4, 5),
                later,
            }));
        Assert.Equal(0, code);
        Assert.Equal(5, stop.LastOffset);
        Assert.Empty(stop.Intercepts);
        Assert.Equal(0, later.ValidCount);

        var portal = Eye();
        portal.ValidCount = 7;
        var blocked = OneSided(4, 5);
        code = VmPSightBlock1163Slice.BlockLines(
            portal, 2, 1, 3, true, false,
            Map<VmPSightBlock1163Slice.PolyLink?>(6, null),
            Map<IReadOnlyList<VmPSightBlock1163Slice.Line>>(6, new List<VmPSightBlock1163Slice.Line> { blocked }));
        Assert.Equal(1, code);
        Assert.True(portal.PortalFound);
        Assert.Single(portal.Intercepts);
        Assert.Equal(4, portal.Intercepts[0].Line!.Id);

        var polyOnly = Eye();
        polyOnly.ValidCount = 3;
        var through = Vertical(11, 5);
        var obj = new VmPSightBlock1163Slice.PolyObject();
        obj.Lines.Add(through);
        var link = new VmPSightBlock1163Slice.PolyLink
        {
            Poly = obj,
            Next = new VmPSightBlock1163Slice.PolyLink(),
        };
        code = VmPSightBlock1163Slice.BlockLines(
            polyOnly, 2, 1, 3, false, true,
            Map<VmPSightBlock1163Slice.PolyLink?>(6, link),
            Map<IReadOnlyList<VmPSightBlock1163Slice.Line>>(6, new List<VmPSightBlock1163Slice.Line> { Vertical(12, 2) }));
        Assert.Equal(1, code);
        Assert.True(polyOnly.PortalFound);
        Assert.Equal(2, polyOnly.Intercepts.Count);
        Assert.Equal(3, obj.ValidCount);
        Assert.Equal(11, polyOnly.Intercepts[0].Line!.Id);
        Assert.Equal(12, polyOnly.Intercepts[1].Line!.Id);

        int again = VmPSightBlock1163Slice.BlockLines(
            polyOnly, 2, 1, 3, false, true,
            Map(6, link),
            Map<IReadOnlyList<VmPSightBlock1163Slice.Line>>(6, new List<VmPSightBlock1163Slice.Line> { through }));
        Assert.Equal(1, again);
        Assert.Equal(2, polyOnly.Intercepts.Count);
    }

    [Fact]
    public void Traverse_OrdersFractionsAndClipsTheLastFloor()
    {
        var check = Eye();
        check.ValidCount = 1;
        VmPSightBlock1163Slice.CheckLine(check, Vertical(8, 8));
        VmPSightBlock1163Slice.CheckLine(check, Vertical(2, 2));
        var seen = new List<int>();
        var fracs = new List<double>();
        check.Visit = hit =>
        {
            seen.Add(hit.Line!.Id);
            fracs.Add(hit.Frac);
            return true;
        };
        Assert.True(VmPSightBlock1163Slice.Traverse(check));
        Assert.Equal(new[] { 2, 8 }, seen);
        Assert.Equal(20d / 100d, fracs[0]);
        Assert.Equal(80d / 100d, fracs[1]);
        Assert.Equal(VmPSightBlock1163Slice.FarFrac, check.Intercepts[0].Frac);
        Assert.Equal(VmPSightBlock1163Slice.FarFrac, check.Intercepts[1].Frac);

        var early = Eye();
        early.ValidCount = 1;
        early.StartFrac = 0.5;
        VmPSightBlock1163Slice.CheckLine(early, Vertical(8, 8));
        VmPSightBlock1163Slice.CheckLine(early, Vertical(2, 2));
        var kept = new List<int>();
        early.Visit = hit =>
        {
            kept.Add(hit.Line!.Id);
            return true;
        };
        Assert.True(VmPSightBlock1163Slice.Traverse(early));
        Assert.Equal(new[] { 8 }, kept);
        Assert.Equal(VmPSightBlock1163Slice.FarFrac, early.Intercepts[1].Frac);

        var edge = Eye();
        edge.ValidCount = 1;
        edge.StartFrac = 0.5;
        VmPSightBlock1163Slice.CheckLine(edge, Vertical(5, 5));
        var edgeSeen = new List<int>();
        edge.Visit = hit =>
        {
            edgeSeen.Add(hit.Line!.Id);
            return true;
        };
        Assert.True(VmPSightBlock1163Slice.Traverse(edge));
        Assert.Equal(new[] { 5 }, edgeSeen);

        var stop = Eye();
        stop.ValidCount = 1;
        VmPSightBlock1163Slice.CheckLine(stop, Vertical(8, 8));
        VmPSightBlock1163Slice.CheckLine(stop, Vertical(2, 2));
        var stopped = new List<int>();
        stop.Visit = hit =>
        {
            stopped.Add(hit.Line!.Id);
            return false;
        };
        Assert.False(VmPSightBlock1163Slice.Traverse(stop));
        Assert.Equal(new[] { 2 }, stopped);

        var floor = Eye();
        floor.LastSector = 3;
        floor.TargetSector = 3;
        floor.EyeZ = 0;
        floor.TopSlope = 10;
        floor.BottomSlope = 6;
        floor.LastZTop = 4;
        floor.LastZBottom = 4;
        floor.Floors.Add(new VmPSightBlock1163Slice.Floor
        {
            Flags = VmPSightBlock1163Slice.FloorExists,
            Bottom = 5,
            Top = 9,
        });
        Assert.False(VmPSightBlock1163Slice.Traverse(floor));

        floor.Floors[0].Flags = VmPSightBlock1163Slice.FloorExists | VmPSightBlock1163Slice.SeeThrough;
        Assert.True(VmPSightBlock1163Slice.Traverse(floor));

        floor.Floors[0].Flags = VmPSightBlock1163Slice.FloorExists;
        floor.Flags = VmPSightBlock1163Slice.IgnoreWater;
        Assert.True(VmPSightBlock1163Slice.Traverse(floor));

        floor.Flags = 0;
        floor.Floors[0].Flags = 0;
        Assert.True(VmPSightBlock1163Slice.Traverse(floor));

        floor.Floors[0].Flags = VmPSightBlock1163Slice.FloorExists;
        floor.TargetSector = 4;
        Assert.True(VmPSightBlock1163Slice.Traverse(floor));

        floor.TargetSector = 3;
        floor.LastZTop = 20;
        floor.LastZBottom = 12;
        floor.TopSlope = 8;
        floor.BottomSlope = 6;
        floor.Floors[0].Bottom = 4;
        floor.Floors[0].Top = 10;
        Assert.False(VmPSightBlock1163Slice.Traverse(floor));
    }

    static VmPSightBlock1163Slice.Check Eye()
    {
        return new VmPSightBlock1163Slice.Check
        {
            Trace = new VmPSightBlock1163Slice.Divline(0, 0, 10, 0),
        };
    }

    static VmPSightBlock1163Slice.Line OneSided(int id, double x)
    {
        var line = Vertical(id, x);
        line.HasBack = false;
        line.Flags = 0;
        return line;
    }

    static VmPSightBlock1163Slice.Line Vertical(int id, double x)
    {
        return Line(id, true, VmPSightBlock1163Slice.TwoSided, x, -5, 10);
    }

    static VmPSightBlock1163Slice.Line Line(int id, bool hasBack, int flags, double x, double y, double dy)
    {
        return new VmPSightBlock1163Slice.Line
        {
            Id = id,
            HasBack = hasBack,
            Flags = flags,
            X = x,
            Y = y,
            Dy = dy,
        };
    }

    static List<T> Map<T>(int count, T atFive)
    {
        var map = new List<T>();
        for (int i = 0; i < count; i++)
            map.Add(default!);
        map[5] = atFive;
        return map;
    }
}
