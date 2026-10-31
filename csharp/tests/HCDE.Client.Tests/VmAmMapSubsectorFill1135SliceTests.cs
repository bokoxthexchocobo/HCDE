namespace HCDE.Client.Tests;

public class VmAmMapSubsectorFill1135SliceTests
{
    private const int Render = VmAmMapSubsectorFill1135Slice.FloorExists | VmAmMapSubsectorFill1135Slice.FloorRenderPlanes;

    [Fact]
    public void Project_MapsWorldToFrameAndSamplesTheCameraSector()
    {
        Assert.False(VmAmMapSubsectorFill1135Slice.UseTexturedFloor(true, true));
        Assert.True(VmAmMapSubsectorFill1135Slice.UseTexturedFloor(true, false));
        var point = VmAmMapSubsectorFill1135Slice.Project(10, 25, 0, 0, 2, 0, 0, 100);
        Assert.Equal((20f, 50f), point);
        Assert.Equal((3d, 4d), VmAmMapSubsectorFill1135Slice.SamplePoint(true, 3, 4, 9, 8));
        Assert.Equal((9d, 8d), VmAmMapSubsectorFill1135Slice.SamplePoint(false, 3, 4, 9, 8));
    }

    [Fact]
    public void SelectRover_TakesTheHighestFloorBelowTheCamera()
    {
        var rovers = new VmAmMapSubsectorFill1135Slice.Rover[]
        {
            new(Render, 255, 200),
            new(Render | VmAmMapSubsectorFill1135Slice.FloorFog, 255, 50),
            new(Render, 0, 40),
            new(Render, 255, 30),
            new(Render, 255, 10)
        };
        Assert.Equal(3, VmAmMapSubsectorFill1135Slice.SelectRover(rovers, 0, 128, 64));
        Assert.Equal(-1, VmAmMapSubsectorFill1135Slice.SelectRover(
            new[] { new VmAmMapSubsectorFill1135Slice.Rover(Render, 255, -5) }, 0, 128, 64));
        Assert.Equal(VmAmMapSubsectorFill1135Slice.FloorPlane, VmAmMapSubsectorFill1135Slice.PlaneSelector(VmAmMapSubsectorFill1135Slice.FloorInvertPlanes));
        Assert.Equal(VmAmMapSubsectorFill1135Slice.CeilingPlane, VmAmMapSubsectorFill1135Slice.PlaneSelector(0));
    }

    [Fact]
    public void TintUnseen_AveragesTowardTheCheatPalette()
    {
        Assert.True(VmAmMapSubsectorFill1135Slice.ShouldTintUnseen(0, 3));
        Assert.False(VmAmMapSubsectorFill1135Slice.ShouldTintUnseen(VmAmMapSubsectorFill1135Slice.SubsectorDrawn, 0));
        Assert.False(VmAmMapSubsectorFill1135Slice.ShouldTintUnseen(0, 4));
        Assert.Equal((132, 110, 95, 194), VmAmMapSubsectorFill1135Slice.TintUnseen(10, 20, 30, 10));
        Assert.Equal(VmAmMapSubsectorFill1135Slice.FadeGray, VmAmMapSubsectorFill1135Slice.FadeColor(VmAmMapSubsectorFill1135Slice.LevelHasFadeTable));
        Assert.True(VmAmMapSubsectorFill1135Slice.ShouldSkipSky(true));
    }

    [Fact]
    public void SectorFill_UsesWhiteScaleAndTheSuppliedAlpha()
    {
        var fill = VmAmMapSubsectorFill1135Slice.SectorFill(0.4);
        Assert.Equal(0.4, fill.Alpha);
        Assert.Equal(255, fill.FloorLight);
        Assert.Equal(1, fill.ScaleX);
        Assert.Equal(0, fill.OriginY);
        Assert.Equal(120, VmAmMapSubsectorFill1135Slice.AutomapTextureAngle(40, 10));
    }
}
