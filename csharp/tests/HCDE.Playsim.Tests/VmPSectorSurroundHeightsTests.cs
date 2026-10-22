using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPSectorSurroundHeightsTests
{
    static void SetFlatFloor(VmPSectorSurroundHeights.MapSector sector, double z)
    {
        sector.FloorTexZ = z;
        sector.FloorPlane.ZAtPoint = (_, _) => z;
    }

    static void SetFlatCeiling(VmPSectorSurroundHeights.MapSector sector, double z)
    {
        sector.CeilingTexZ = z;
        sector.CeilingPlane.ZAtPoint = (_, _) => z;
    }

    static (VmPSectorSurroundHeights.MapSector a, VmPSectorSurroundHeights.MapSector b, VmPSectorSurroundHeights.MapLine line) Pair(
        double floorA,
        double floorB)
    {
        var a = new VmPSectorSurroundHeights.MapSector { SectorIndex = 0 };
        var b = new VmPSectorSurroundHeights.MapSector { SectorIndex = 1 };
        SetFlatFloor(a, floorA);
        SetFlatFloor(b, floorB);
        var line = new VmPSectorSurroundHeights.MapLine { FrontSector = a, BackSector = b };
        a.Lines.Add(line);
        b.Lines.Add(line);
        return (a, b, line);
    }

    [Fact]
    public void FindLowestFloorSurrounding_PrefersLowerNeighbor()
    {
        var (center, _, _) = Pair(64, 32);
        Assert.Equal(32, VmPSectorSurroundHeights.FindLowestFloorSurrounding(center));
    }

    [Fact]
    public void FindHighestFloorSurrounding_PrefersHigherNeighbor()
    {
        var (center, _, _) = Pair(32, 96);
        Assert.Equal(96, VmPSectorSurroundHeights.FindHighestFloorSurrounding(center));
    }

    [Fact]
    public void FindNextHighestFloor_SkipsLinkedNeighbors()
    {
        var (center, neighbor, _) = Pair(32, 48);
        var higher = new VmPSectorSurroundHeights.MapSector { SectorIndex = 2 };
        SetFlatFloor(higher, 80);
        var line2 = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = higher };
        center.Lines.Add(line2);
        higher.Lines.Add(line2);

        center.LinkedPlane = new VmLinkedSectorPlane();
        VmLinkedSectorAttach.AddSingleSector(center.LinkedPlane, 2, VmLinkedSectorLinkType.Floor);

        Assert.Equal(48, VmPSectorSurroundHeights.FindNextHighestFloor(center));
    }

    [Fact]
    public void FindNextLowestFloor_PicksClosestDrop()
    {
        var center = new VmPSectorSurroundHeights.MapSector { SectorIndex = 0 };
        SetFlatFloor(center, 100);
        var low = new VmPSectorSurroundHeights.MapSector { SectorIndex = 1 };
        SetFlatFloor(low, 80);
        var lower = new VmPSectorSurroundHeights.MapSector { SectorIndex = 2 };
        SetFlatFloor(lower, 40);
        var line1 = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = low };
        var line2 = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = lower };
        center.Lines.Add(line1);
        center.Lines.Add(line2);
        low.Lines.Add(line1);
        lower.Lines.Add(line2);

        Assert.Equal(80, VmPSectorSurroundHeights.FindNextLowestFloor(center));
    }

    [Fact]
    public void FindNextLowestCeiling_AndHighestCeiling()
    {
        var center = new VmPSectorSurroundHeights.MapSector { SectorIndex = 0 };
        SetFlatCeiling(center, 200);
        var low = new VmPSectorSurroundHeights.MapSector { SectorIndex = 1 };
        SetFlatCeiling(low, 160);
        var high = new VmPSectorSurroundHeights.MapSector { SectorIndex = 2 };
        SetFlatCeiling(high, 240);
        var line1 = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = low };
        var line2 = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = high };
        center.Lines.Add(line1);
        center.Lines.Add(line2);
        low.Lines.Add(line1);
        high.Lines.Add(line2);

        Assert.Equal(160, VmPSectorSurroundHeights.FindNextLowestCeiling(center));
        Assert.Equal(240, VmPSectorSurroundHeights.FindNextHighestCeiling(center));
    }

    [Fact]
    public void EmptyLines_ReturnsPlaneTexZ()
    {
        var sector = new VmPSectorSurroundHeights.MapSector();
        sector.FloorTexZ = 12;
        sector.CeilingTexZ = 200;
        Assert.Equal(12, VmPSectorSurroundHeights.FindLowestFloorSurrounding(sector));
        Assert.Equal(200, VmPSectorSurroundHeights.FindNextHighestCeiling(sector));
    }

    [Fact]
    public void FindLowestFloorSurrounding_WritesSpotVertex()
    {
        var (center, _, line) = Pair(64, 16);
        line.V2.X = 99;
        var spot = new VmPSectorSurroundHeights.Vertex();
        VmPSectorSurroundHeights.FindLowestFloorSurrounding(center, spot);
        Assert.True(spot.X == line.V1.X || spot.X == line.V2.X);
    }
}
