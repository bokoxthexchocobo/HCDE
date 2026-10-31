namespace HCDE.Client.Tests;

public class VmAmMapPolyShade1136SliceTests
{
    [Fact]
    public void HoleRing_TriangulatesOnlyWhenFlaggedAndMoreThanThreeLines()
    {
        Assert.Equal((0d, 0d), VmAmMapPolyShade1136Slice.ZeroOrigin());
        Assert.True(VmAmMapPolyShade1136Slice.ShouldDrawPolygon(true));
        Assert.False(VmAmMapPolyShade1136Slice.ShouldDrawPolygon(false));
        Assert.True(VmAmMapPolyShade1136Slice.NeedsHoleTriangulation(VmAmMapPolyShade1136Slice.Hole, 4));
        Assert.False(VmAmMapPolyShade1136Slice.NeedsHoleTriangulation(VmAmMapPolyShade1136Slice.Hole, 3));
        Assert.False(VmAmMapPolyShade1136Slice.NeedsHoleTriangulation(0, 6));
        var ring = VmAmMapPolyShade1136Slice.CopyHoleRing(new[] { (1d, 2d), (3d, 4d) });
        Assert.Equal((3d, 4d), ring[1]);
    }

    [Fact]
    public void FadeLevel_UsesTheSoftRampOrTheLinearHardwareScale()
    {
        Assert.Equal(0.53125, VmAmMapPolyShade1136Slice.FadeLevel(true, 128));
        Assert.Equal(0d, VmAmMapPolyShade1136Slice.FadeLevel(true, 255));
        Assert.Equal(1d, VmAmMapPolyShade1136Slice.FadeLevel(true, 0));
        Assert.Equal(1.0 - 128 / 255.0, VmAmMapPolyShade1136Slice.LinearFade(128));
        Assert.Equal(1d, VmAmMapPolyShade1136Slice.LinearFade(-4));
        Assert.Equal(0d, VmAmMapPolyShade1136Slice.LinearFade(400));
        Assert.Equal((0.5, 2d), VmAmMapPolyShade1136Slice.TextureScale(2, 4, 1));
    }

    [Fact]
    public void CheckSecret_PrefersARevealedFrontSector()
    {
        Assert.Equal(0, VmAmMapPolyShade1136Slice.CheckSecret(false, true, true, false, false, false, false, 1, 0));
        Assert.Equal(1, VmAmMapPolyShade1136Slice.CheckSecret(true, true, true, false, true, true, false, 1, 0));
        Assert.Equal(2, VmAmMapPolyShade1136Slice.CheckSecret(true, true, true, true, false, false, false, 2, 0));
        Assert.Equal(2, VmAmMapPolyShade1136Slice.CheckSecret(true, false, false, false, true, true, true, 2, 0));
        Assert.Equal(0, VmAmMapPolyShade1136Slice.CheckSecret(true, true, true, true, false, false, false, 2, VmAmMapPolyShade1136Slice.LineSecret));
        Assert.Equal(0, VmAmMapPolyShade1136Slice.CheckSecret(true, true, true, true, false, false, false, 0, 0));
    }

    [Fact]
    public void DebugStrokes_RotateSegsAndMarkTheShownSubsector()
    {
        var plain = VmAmMapPolyShade1136Slice.OrientedSeg(1, 2, 3, 4, false, (x, y) => (x + 1, y));
        Assert.Equal(1d, plain.Ax);
        var turned = VmAmMapPolyShade1136Slice.OrientedSeg(1, 2, 3, 4, true, (x, y) => (x + 1, y));
        Assert.Equal(2d, turned.Ax);
        Assert.Equal(4d, turned.Bx);
        Assert.True(VmAmMapPolyShade1136Slice.ShouldRotate(2, true));
        Assert.False(VmAmMapPolyShade1136Slice.ShouldRotate(2, false));
        Assert.False(VmAmMapPolyShade1136Slice.ShowSubsector(-1, 4));
        Assert.True(VmAmMapPolyShade1136Slice.ShowSubsector(1, 4));
        Assert.False(VmAmMapPolyShade1136Slice.ShowSubsector(4, 4));
        Assert.Equal((255, 255, 0), VmAmMapPolyShade1136Slice.SubsectorYellow());
        Assert.Equal((255, 0, 0), VmAmMapPolyShade1136Slice.PolyNodeRed());
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, VmAmMapPolyShade1136Slice.YellowSegIndexes(5));
        Assert.True(VmAmMapPolyShade1136Slice.PolyNodeBelongs(3, 3));
        Assert.False(VmAmMapPolyShade1136Slice.PolyNodeBelongs(2, 3));
    }
}
