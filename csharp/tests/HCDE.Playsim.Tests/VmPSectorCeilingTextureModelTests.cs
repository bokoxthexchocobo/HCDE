using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPSectorCeilingTextureModelTests
{
    static void FlatCeiling(VmPSectorSurroundHeights.MapSector s, double z)
    {
        s.CeilingTexZ = z;
        s.CeilingPlane.ZAtPoint = (_, _) => z;
    }

    static void FlatFloor(VmPSectorSurroundHeights.MapSector s, double z)
    {
        s.FloorTexZ = z;
        s.FloorPlane.ZAtPoint = (_, _) => z;
    }

    static (VmPSectorSurroundHeights.MapSector center, VmPSectorSurroundHeights.MapSector neighbor) PairCeil(double centerZ, double neighborZ)
    {
        var center = new VmPSectorSurroundHeights.MapSector();
        var neighbor = new VmPSectorSurroundHeights.MapSector();
        FlatCeiling(center, centerZ);
        FlatCeiling(neighbor, neighborZ);
        var line = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = neighbor };
        center.Lines.Add(line);
        neighbor.Lines.Add(line);
        return (center, neighbor);
    }

    [Fact]
    public void FindLowestCeilingSurrounding_PrefersLowerNeighbor()
    {
        var (center, _) = PairCeil(128, 96);
        Assert.Equal(96, VmPSectorCeilingTextureModel.FindLowestCeilingSurrounding(center));
    }

    [Fact]
    public void FindHighestCeilingSurrounding_PrefersHigherNeighbor()
    {
        var (center, _) = PairCeil(96, 160);
        Assert.Equal(160, VmPSectorCeilingTextureModel.FindHighestCeilingSurrounding(center));
    }

    [Fact]
    public void FindShortestTextureAround_UsesSmallestLowerTexture()
    {
        var sec = new VmPSectorCeilingTextureModel.LightSector();
        sec.Level.TextureIdIsValid = id => id >= 0;
        sec.Level.TextureDisplayHeight = id => id == 1 ? 32 : 64;
        sec.Level.FallbackTextureHeight = 128;
        var line = new VmPSectorCeilingTextureModel.TextureLine
        {
            Flags = VmPSectorCeilingTextureModel.MlTwosided,
            FrontSector = sec,
            BackSector = null,
        };
        line.Side0.BottomTextureId = 1;
        line.Side1.BottomTextureId = 2;
        sec.Lines.Add(line);

        Assert.Equal(32, VmPSectorCeilingTextureModel.FindShortestTextureAround(sec));
    }

    [Fact]
    public void FindModelFloorSector_MatchesNeighborHeight()
    {
        var center = new VmPSectorSurroundHeights.MapSector();
        var neighbor = new VmPSectorSurroundHeights.MapSector();
        FlatFloor(center, 0);
        FlatFloor(neighbor, 48);
        var line = new VmPSectorSurroundHeights.MapLine { FrontSector = center, BackSector = neighbor };
        center.Lines.Add(line);
        neighbor.Lines.Add(line);

        Assert.Same(neighbor, VmPSectorCeilingTextureModel.FindModelFloorSector(center, 48));
    }

    [Fact]
    public void FindMinSurroundingLight_TakesMinimum()
    {
        var center = new VmPSectorCeilingTextureModel.LightSector { LightLevel = 200 };
        var dim = new VmPSectorCeilingTextureModel.LightSector { LightLevel = 80 };
        var line = new VmPSectorCeilingTextureModel.TextureLine { FrontSector = center, BackSector = dim };
        center.Lines.Add(line);
        dim.Lines.Add(line);

        Assert.Equal(80, VmPSectorCeilingTextureModel.FindMinSurroundingLight(center, 255));
    }

    [Fact]
    public void FindHighestFloorPoint_FlatUsesNegD()
    {
        var sec = new VmPSectorCeilingTextureModel.LightSector { FloorIsSlope = false, FloorPlaneD = -64 };
        var vtx = new VmPSectorSurroundHeights.Vertex();
        Assert.Equal(64, VmPSectorCeilingTextureModel.FindHighestFloorPoint(sec, vtx));
    }
}
