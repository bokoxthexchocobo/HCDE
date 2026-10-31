namespace HCDE.Client.Tests;

public class VmAmMapClipLine1132SliceTests
{
    [Fact]
    public void Scale_UsesWideningIntegerDivision()
    {
        Assert.Equal(15, VmAmMapClipLine1132Slice.Scale(10, 3, 2));
        Assert.Equal(2, VmAmMapClipLine1132Slice.Scale(5, 1, 2));
    }

    [Fact]
    public void TryClip_RejectsASegmentThatIsEntirelyAboveTheMapWindow()
    {
        var visible = VmAmMapClipLine1132Slice.TryClip(
            50, 200, 60, 180, 0, 0, 100, 100, 100, 100, 1,
            out _, out _, out _, out _);
        Assert.False(visible);
    }

    [Fact]
    public void TryClip_CutsAVerticalSegmentToTheTopEdge()
    {
        var visible = VmAmMapClipLine1132Slice.TryClip(
            50, 50, 50, 150, 0, 0, 100, 100, 100, 100, 1,
            out var ax, out var ay, out var bx, out var by);
        Assert.True(visible);
        Assert.Equal((50, 50), (ax, ay));
        Assert.Equal((50, 0), (bx, by));
        Assert.Equal((58, 54), VmAmMapClipLine1132Slice.ScreenStart(8, 4, ax, ay));
    }

    [Fact]
    public void TryClip_CutsAHorizontalSegmentToTheLeftEdge()
    {
        var visible = VmAmMapClipLine1132Slice.TryClip(
            -50, 50, 50, 50, 0, 0, 100, 100, 100, 100, 1,
            out var ax, out var ay, out var bx, out var by);
        Assert.True(visible);
        Assert.Equal((0, 50), (ax, ay));
        Assert.Equal((50, 50), (bx, by));
    }

    [Fact]
    public void FrameOutcode_MarksTheCornerOutside()
    {
        var code = VmAmMapClipLine1132Slice.FrameOutcode(-1, -1, 100, 100);
        Assert.Equal(VmAmMapClipLine1132Slice.Left | VmAmMapClipLine1132Slice.Top, code);
    }
}
