namespace HCDE.Playsim.Tests;

public class VmPSightOpening1162SliceTests
{
    [Fact]
    public void Begin_RaisesTheEyeAndShiftsOnlyAcrossGroups()
    {
        var task = new VmPSightOpening1162Slice.SightTask(0.25, 3, -2, 1, 1);
        var same = VmPSightOpening1162Slice.Begin(
            new(10, 20, 30, 8, 1),
            new(14, 24, 99, 16, 1),
            7, 4, task, 9,
            (_, _) => throw new InvalidOperationException());
        Assert.Equal(10, same.StartX);
        Assert.Equal(20, same.StartY);
        Assert.Equal(36, same.StartZ);
        Assert.Equal(14, same.EndX);
        Assert.Equal(24, same.EndY);
        Assert.Equal(4, same.TraceDx);
        Assert.Equal(4, same.TraceDy);
        Assert.Equal(36, same.LastZTop);
        Assert.Equal(36, same.LastZBottom);
        Assert.Equal(0.25, same.StartFrac);
        Assert.Equal(4, same.LastSector);
        Assert.Equal(7, same.TargetId);
        Assert.Equal(3, same.TopSlope);
        Assert.Equal(-2, same.BottomSlope);
        Assert.Equal(9, same.Flags);
        Assert.Equal(1, same.PortalDir);
        Assert.Equal(1, same.PortalGroup);
        Assert.False(same.PortalFound);
        Assert.Equal(VmPSightOpening1162Slice.SeeThrough, same.SeeThroughFlags);
        Assert.Equal(128, same.Intercepts.Capacity);
        Assert.Equal(32, same.Portals.Capacity);
        Assert.Equal("BotCheckSight", VmPSightOpening1162Slice.BotSightStream);
        Assert.Equal("CheckSight", VmPSightOpening1162Slice.SightStream);

        var shifted = VmPSightOpening1162Slice.Begin(
            new(10, 20, 30, 8, 1),
            new(14, 24, 99, 16, 4),
            7, 4, task with { PortalGroup = 2 }, 9,
            (from, to) =>
            {
                Assert.NotEqual(from, to);
                if (from == 1 && to == 2)
                    return (5, -2);
                if (from == 4 && to == 2)
                    return (1, 1);
                throw new InvalidOperationException();
            });
        Assert.Equal(15, shifted.StartX);
        Assert.Equal(18, shifted.StartY);
        Assert.Equal(36, shifted.StartZ);
        Assert.Equal(15, shifted.EndX);
        Assert.Equal(25, shifted.EndY);
        Assert.Equal(0, shifted.TraceDx);
        Assert.Equal(7, shifted.TraceDy);
        Assert.Equal(2, shifted.PortalGroup);
    }

    [Fact]
    public void Swap_ExchangesFrontAndBackBits()
    {
        Assert.Equal(VmPSightOpening1162Slice.TopBack | VmPSightOpening1162Slice.BottomBack,
            VmPSightOpening1162Slice.SwapSides(VmPSightOpening1162Slice.TopFront | VmPSightOpening1162Slice.BottomFront));
        Assert.Equal(VmPSightOpening1162Slice.TopFront | VmPSightOpening1162Slice.BottomFront,
            VmPSightOpening1162Slice.SwapSides(VmPSightOpening1162Slice.TopBack | VmPSightOpening1162Slice.BottomBack));
        Assert.Equal(15, VmPSightOpening1162Slice.SwapSides(15));
        Assert.Equal(VmPSightOpening1162Slice.TopBack, VmPSightOpening1162Slice.SwapSides(VmPSightOpening1162Slice.TopFront | 16));
    }

    [Fact]
    public void Open_OneSidedUsesTheSwappedPlanesAndAlwaysCloses()
    {
        var closed = VmPSightOpening1162Slice.Open(0, false, false, false, false, false, 9, 8, 7, 6);
        Assert.Equal(0, closed.Top);
        Assert.Equal(0, closed.Bottom);
        Assert.Equal(0, closed.Range);
        Assert.Equal(0, closed.PortalFlags);

        int connect = VmPSightOpening1162Slice.PortalConnect;
        var blocked = VmPSightOpening1162Slice.Open(connect, false, true, true, false, false, 40, 5, 0, 0);
        Assert.Equal(5, blocked.Top);
        Assert.Equal(40, blocked.Bottom);
        Assert.Equal(0, blocked.Range);
        Assert.Equal(0, blocked.PortalFlags);

        var open = VmPSightOpening1162Slice.Open(connect, false, false, false, true, true, 40, 5, 0, 0);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMax, open.Top);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMin, open.Bottom);
        Assert.Equal(0, open.Range);
        Assert.Equal(VmPSightOpening1162Slice.TopFront | VmPSightOpening1162Slice.BottomFront, open.PortalFlags);

        var ceilingOnly = VmPSightOpening1162Slice.Open(connect, false, false, true, true, true, 40, 5, 0, 0);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMax, ceilingOnly.Top);
        Assert.Equal(40, ceilingOnly.Bottom);
        Assert.Equal(VmPSightOpening1162Slice.TopFront, ceilingOnly.PortalFlags);
        Assert.Equal(0, ceilingOnly.Range);
    }

    [Fact]
    public void Open_TwoSidedWindowIsTheOverlapNotTheDistance()
    {
        var window = VmPSightOpening1162Slice.Open(0, true, true, true, true, true, 100, 10, 80, 20);
        Assert.Equal(80, window.Top);
        Assert.Equal(20, window.Bottom);
        Assert.Equal(1, window.Range);
        Assert.Equal(0, window.PortalFlags);

        var shut = VmPSightOpening1162Slice.Open(0, true, true, true, true, true, 8, 10, 30, 20);
        Assert.Equal(8, shut.Top);
        Assert.Equal(20, shut.Bottom);
        Assert.Equal(0, shut.Range);

        var equal = VmPSightOpening1162Slice.Open(0, true, true, true, true, true, 12, 12, 40, 4);
        Assert.Equal(12, equal.Top);
        Assert.Equal(12, equal.Bottom);
        Assert.Equal(0, equal.Range);

        int connect = VmPSightOpening1162Slice.PortalConnect;
        var portals = VmPSightOpening1162Slice.Open(connect, true, false, false, false, false, 50, 4, 60, 8);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMax, portals.Top);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMin, portals.Bottom);
        Assert.Equal(1, portals.Range);
        Assert.Equal(15, portals.PortalFlags);

        var frontCeilBlocked = VmPSightOpening1162Slice.Open(connect, true, true, false, false, false, 50, 4, 60, 8);
        Assert.Equal(50, frontCeilBlocked.Top);
        Assert.Equal(VmPSightOpening1162Slice.LineOpenMin, frontCeilBlocked.Bottom);
        Assert.Equal(1, frontCeilBlocked.Range);
        Assert.Equal(
            VmPSightOpening1162Slice.TopBack | VmPSightOpening1162Slice.BottomFront | VmPSightOpening1162Slice.BottomBack,
            frontCeilBlocked.PortalFlags);
    }
}
