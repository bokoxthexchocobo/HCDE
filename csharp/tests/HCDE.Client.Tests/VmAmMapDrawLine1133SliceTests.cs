namespace HCDE.Client.Tests;

public class VmAmMapDrawLine1133SliceTests
{
    [Fact]
    public void DrawThickness_PrefersAPositiveRequestAndSplitsAntialiasAlpha()
    {
        Assert.Equal(4, VmAmMapDrawLine1133Slice.DrawThickness(4, 1));
        Assert.Equal(1, VmAmMapDrawLine1133Slice.DrawThickness(0, 1));
        Assert.Equal(3, VmAmMapDrawLine1133Slice.AntialiasDivisor(3));
        Assert.Equal(127, VmAmMapDrawLine1133Slice.AlphaByte(1, 2));
        Assert.Equal((12, 9), VmAmMapDrawLine1133Slice.ScreenEnd(2, 4, 10, 5));
    }

    [Fact]
    public void AntialiasOffsets_RepeatTheTopCenterOnTheLastThickStroke()
    {
        var thick = VmAmMapDrawLine1133Slice.AntialiasOffsets(4);
        Assert.Equal(9, thick.Count);
        Assert.Equal((0, -1), thick[^1]);
        Assert.True(VmAmMapDrawLine1133Slice.UseThickLine(true, 4));
        var thin = VmAmMapDrawLine1133Slice.AntialiasOffsets(2);
        Assert.Equal(5, thin.Count);
        Assert.DoesNotContain((-1, -1), thin);
        Assert.False(VmAmMapDrawLine1133Slice.UseThickLine(false, 1));
    }

    [Fact]
    public void VerticalGridLines_StepByMapBlocksFromTheCeiledOrigin()
    {
        Assert.Equal(5, VmAmMapDrawLine1133Slice.CoverLength(3, 4));
        Assert.Equal(128, VmAmMapDrawLine1133Slice.GridStart(100, 0, 0));
        var lines = VmAmMapDrawLine1133Slice.VerticalGridLines(0, 0, 100, 100, 0);
        Assert.Single(lines);
        Assert.Equal(0, lines[0].Ax);
        Assert.Equal(lines[0].Ax, lines[0].Bx);
        Assert.True(VmAmMapDrawLine1133Slice.ShouldRotateGrid(2, true));
        Assert.False(VmAmMapDrawLine1133Slice.ShouldRotateGrid(2, false));
    }

    [Fact]
    public void KeepOriginalSector_WhenThereIsNoHeightSector()
    {
        Assert.Equal(0x00ff00u, VmAmMapDrawLine1133Slice.PaletteColor(1, new uint[] { 1, 0x00ff00 }));
        Assert.True(VmAmMapDrawLine1133Slice.KeepOriginalSector(false));
        Assert.False(VmAmMapDrawLine1133Slice.KeepOriginalSector(true));
    }
}
