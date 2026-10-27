namespace HCDE.Playsim.Tests;

public class VmPLnspecScrollSector929SliceTests
{
    [Fact]
    public void Panning_CombinesIntAndFrac()
    {
        var (x, _) = VmPLnspecScrollSector929Slice.CeilingPanning(1, 50, 0, 0);
        Assert.Equal(1.5, x);
    }

    [Fact]
    public void FloorScale_InvertsNonZero()
    {
        var scale = VmPLnspecScrollSector929Slice.InvertScaleComponent(2, 0);
        Assert.Equal(0.5, scale);
    }

    [Fact]
    public void Scale2_Uses65536Factors()
    {
        var inv = VmPLnspecScrollSector929Slice.InvertScaleFactor65536(65536);
        Assert.Equal(1.0, inv);
    }

    [Fact]
    public void CeilingScale_MatchesFloorInvert()
    {
        var floor = VmPLnspecScrollSector929Slice.SectorSetFloorScale(1, 2, 0, 2, 0);
        var ceiling = VmPLnspecScrollSector929Slice.SectorSetCeilingScale(1, 2, 0, 2, 0);
        Assert.Equal(floor.XScale, ceiling.XScale);
    }

    [Fact]
    public void SectorSetColor_CarriesDesaturate()
    {
        var color = VmPLnspecScrollSector929Slice.SectorSetColor(3, 10, 20, 30, 5);
        Assert.Equal(5, color.Desaturate);
    }

    [Fact]
    public void ShouldApplyScale2_SkipsZeroFactor()
    {
        Assert.False(VmPLnspecScrollSector929Slice.ShouldApplyScale2X(0, 1.0));
    }

    [Fact]
    public void BuildFloorPanning_MarksFloorPlane()
    {
        var pan = VmPLnspecScrollSector929Slice.BuildFloorPanning(2, 0, 25, 1, 0);
        Assert.False(pan.CeilingPlane);
        Assert.Equal(0.25, pan.XOffset);
    }
}
