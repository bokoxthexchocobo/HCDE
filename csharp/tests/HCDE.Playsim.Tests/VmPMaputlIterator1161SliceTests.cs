namespace HCDE.Playsim.Tests;

public class VmPMaputlIterator1161SliceTests
{
    [Fact]
    public void Actor_RewritesRadiusAndSkipsCollectWhenInited()
    {
        int calls = 0;
        var open = VmPMaputlIterator1161Slice.FromActor(
            false, 10, 20, 100, 130, 6, -1, 3, null, null, null,
            (state, top, radius) =>
            {
                calls++;
                Assert.Equal(100, state.Z);
                Assert.Equal(10, state.X);
                Assert.Equal(20, state.Y);
                Assert.Equal(3, state.BaseGroup);
                Assert.Equal(130, top);
                Assert.Equal(-1, radius);
            });
        Assert.Equal(1, calls);
        Assert.Equal(6, open.Z);
        Assert.Equal(-1, open.Index);
        Assert.Equal(0, open.PortalFlags);
        Assert.True(open.HashCleared);
        Assert.False(open.InitClearsHash);
        Assert.Equal(1, open.InitCount);
        Assert.Equal(26, open.BoxTop);
        Assert.Equal(4, open.BoxLeft);
        Assert.Equal(16, open.BoxRight);
        Assert.Equal(14, open.BoxBottom);

        var kept = VmPMaputlIterator1161Slice.FromActor(
            true, 10, 20, 100, 130, 6, 2, 3, new[] { 7 }, null, null,
            (_, _, _) => calls++);
        Assert.Equal(1, calls);
        Assert.Equal(2, kept.Z);
        Assert.Equal(new[] { 7 }, kept.Checklist);
        Assert.Equal(22, kept.BoxTop);
        Assert.Equal(8, kept.BoxLeft);
        Assert.Equal(12, kept.BoxRight);
        Assert.Equal(18, kept.BoxBottom);
    }

    [Fact]
    public void Point_ReadsTheSectorAndStoresTheGivenRadius()
    {
        int lookups = 0;
        double seenUpper = 0;
        double seenZ = 0;
        var found = VmPMaputlIterator1161Slice.FromPoint(
            false, 8, 9, 3, 10, 2, false, 99,
            (x, y) =>
            {
                lookups++;
                Assert.Equal(8, x);
                Assert.Equal(9, y);
                return 4;
            },
            null, null, null,
            (state, upper) =>
            {
                seenUpper = upper;
                seenZ = state.Z;
                Assert.Equal(4, state.BaseGroup);
            });
        Assert.Equal(1, lookups);
        Assert.Equal(13, seenUpper);
        Assert.Equal(3, seenZ);
        Assert.Equal(2, found.Z);
        Assert.Equal(4, found.BaseGroup);
        Assert.Equal(11, found.BoxTop);
        Assert.Equal(6, found.BoxLeft);
        Assert.Equal(10, found.BoxRight);
        Assert.Equal(7, found.BoxBottom);

        int blocked = 0;
        var negative = VmPMaputlIterator1161Slice.FromPoint(
            false, 8, 9, 3, 10, -1, true, 6,
            (_, _) =>
            {
                blocked++;
                return 0;
            },
            null, null, null,
            (state, upper) =>
            {
                Assert.Equal(6, state.BaseGroup);
                Assert.Equal(3, state.Z);
                Assert.Equal(13, upper);
            });
        Assert.Equal(0, blocked);
        Assert.Equal(-1, negative.Z);
        Assert.Equal(6, negative.BaseGroup);
        Assert.Equal(8, negative.BoxTop);
        Assert.Equal(9, negative.BoxLeft);
        Assert.Equal(7, negative.BoxRight);
        Assert.Equal(10, negative.BoxBottom);
    }

    [Fact]
    public void Next_ReturnsTheBaseThingThenUpperAndLowerGroups()
    {
        var groups = new Dictionary<int, List<VmPMaputlIterator1161Slice.Thing>>
        {
            [1] = new() { new(10, 1) },
            [2] = new() { new(20, 5) },
            [3] = new() { new(40, 3) },
            [5] = new() { new(30, 5) },
        };
        var state = VmPMaputlIterator1161Slice.FromActor(
            false, 10, 20, 100, 130, 6, 4, 1, null, Offset, groups,
            (s, _, _) =>
            {
                s.Checklist.Add(VmPMaputlIterator1161Slice.PortalUpper | 2);
                s.Checklist.Add(VmPMaputlIterator1161Slice.PortalLower | 5);
            });

        var first = VmPMaputlIterator1161Slice.Next(state);
        Assert.True(first.Found);
        Assert.Equal(10, first.ThingId);
        Assert.Equal(10, first.X);
        Assert.Equal(20, first.Y);
        Assert.Equal(4, first.Z);
        Assert.Equal(0, first.PortalFlags);
        Assert.Equal(-1, state.Index);
        Assert.Equal(1, state.InitCount);

        var upper = VmPMaputlIterator1161Slice.Next(state);
        Assert.True(upper.Found);
        Assert.Equal(20, upper.ThingId);
        Assert.Equal(17, upper.X);
        Assert.Equal(17, upper.Y);
        Assert.Equal(4, upper.Z);
        Assert.Equal(VmPMaputlIterator1161Slice.NoFloor, upper.PortalFlags);
        Assert.Equal(0, state.Index);
        Assert.Equal(2, state.InitCount);
        Assert.False(state.InitClearsHash);
        Assert.True(state.HashCleared);
        Assert.Equal(25, state.BoxTop);
        Assert.Equal(7, state.BoxLeft);
        Assert.Equal(15, state.BoxRight);
        Assert.Equal(17, state.BoxBottom);

        var lower = VmPMaputlIterator1161Slice.Next(state);
        Assert.True(lower.Found);
        Assert.Equal(30, lower.ThingId);
        Assert.Equal(17, lower.X);
        Assert.Equal(17, lower.Y);
        Assert.Equal(VmPMaputlIterator1161Slice.NoCeiling, lower.PortalFlags);
        Assert.Equal(1, state.Index);
        Assert.Equal(3, state.InitCount);
        Assert.Equal(21, state.BoxTop);
        Assert.Equal(13, state.BoxLeft);
        Assert.Equal(21, state.BoxRight);
        Assert.Equal(13, state.BoxBottom);

        var done = VmPMaputlIterator1161Slice.Next(state);
        Assert.False(done.Found);
        Assert.Equal(1, state.Index);

        var empty = VmPMaputlIterator1161Slice.FromActor(
            true, 10, 20, 100, 130, 6, 4, 1, null, Offset, null, null);
        var none = VmPMaputlIterator1161Slice.Next(empty);
        Assert.False(none.Found);
        Assert.Equal(-1, empty.Index);

        var flat = VmPMaputlIterator1161Slice.FromActor(
            true, 10, 20, 100, 130, 6, 4, 1,
            new[] { VmPMaputlIterator1161Slice.PortalFlat | 3 },
            Offset, groups, null);
        var baseFirst = VmPMaputlIterator1161Slice.Next(flat);
        Assert.True(baseFirst.Found);
        Assert.Equal(10, baseFirst.ThingId);
        Assert.Equal(0, baseFirst.PortalFlags);
        Assert.Equal(-1, flat.Index);
        var both = VmPMaputlIterator1161Slice.Next(flat);
        Assert.True(both.Found);
        Assert.Equal(40, both.ThingId);
        Assert.Equal(14, both.X);
        Assert.Equal(20, both.Y);
        Assert.Equal(0, both.PortalFlags);
        Assert.Equal(0, flat.Index);
        Assert.Equal(24, flat.BoxTop);
        Assert.Equal(10, flat.BoxLeft);
        Assert.Equal(18, flat.BoxRight);
        Assert.Equal(16, flat.BoxBottom);
    }

    [Fact]
    public void Lines_KeepCrossingsInsideTheSpan()
    {
        var trace = new VmPMaputlIterator1161Slice.Divline(0, 0, 10, 0);
        var axis = new VmPMaputlIterator1161Slice.Divline(0, 0, 1, 0);
        Assert.Equal(0, VmPMaputlIterator1161Slice.PointOnDivlineSide(0, 1d / 65536d, axis));
        Assert.Equal(1, VmPMaputlIterator1161Slice.PointOnDivlineSide(0, 2d / 65536d, axis));
        Assert.Equal(0, VmPMaputlIterator1161Slice.InterceptAlongTrace(
            trace, new VmPMaputlIterator1161Slice.Divline(0, 1, 5, 0)));
        Assert.Equal(0.5, VmPMaputlIterator1161Slice.InterceptAlongTrace(
            trace, new VmPMaputlIterator1161Slice.Divline(5, -5, 0, 10)));

        var traverse = VmPMaputlIterator1161Slice.CreateTraverse(trace, 0.25);
        Assert.Equal(128, traverse.Intercepts.Capacity);
        traverse.Lines.Add(new(1, 5, -5, 0, 10));
        traverse.Lines.Add(new(2, 0, 1, 10, 0));
        traverse.Lines.Add(new(3, -5, -5, 0, 10));
        traverse.Lines.Add(new(4, 20, -5, 0, 10));
        traverse.Lines.Add(new(5, 2.5, -5, 0, 10));
        traverse.Lines.Add(new(6, 10, -5, 0, 10));
        VmPMaputlIterator1161Slice.AddLineIntercepts(traverse, 3, 4);

        Assert.Equal(3, traverse.BlockX);
        Assert.Equal(4, traverse.BlockY);
        Assert.Equal(3, traverse.Intercepts.Count);
        Assert.Equal(1, traverse.Intercepts[0].LineId);
        Assert.Equal(0.5, traverse.Intercepts[0].Frac);
        Assert.True(traverse.Intercepts[0].IsALine);
        Assert.False(traverse.Intercepts[0].Done);
        Assert.Equal(5, traverse.Intercepts[1].LineId);
        Assert.Equal(0.25, traverse.Intercepts[1].Frac);
        Assert.Equal(6, traverse.Intercepts[2].LineId);
        Assert.Equal(1, traverse.Intercepts[2].Frac);

        var later = VmPMaputlIterator1161Slice.CreateTraverse(trace, 0.5);
        later.Lines.Add(new(5, 2.5, -5, 0, 10));
        later.Lines.Add(new(1, 5, -5, 0, 10));
        later.Lines.Add(new(6, 10, -5, 0, 10));
        VmPMaputlIterator1161Slice.AddLineIntercepts(later, 3, 4);
        Assert.Equal(2, later.Intercepts.Count);
        Assert.Equal(1, later.Intercepts[0].LineId);
        Assert.Equal(6, later.Intercepts[1].LineId);
    }

    static (double X, double Y) Offset(int from, int to)
    {
        Assert.Equal(1, from);
        Assert.NotEqual(from, to);
        if (to == 2)
            return (1, 1);
        if (to == 5)
            return (7, -3);
        if (to == 3)
            return (4, 0);
        throw new InvalidOperationException(to.ToString());
    }
}
