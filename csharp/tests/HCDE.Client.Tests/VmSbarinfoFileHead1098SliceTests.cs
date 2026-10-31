namespace HCDE.Client.Tests;

public class VmSbarinfoFileHead1098SliceTests
{
    [Fact]
    public void PackCoordinate_StoresRelativeCenterInSignBit()
    {
        var packed = VmSbarinfoFileHead1098Slice.PackCoordinate(42, relativeCenter: true);
        var (value, rel) = VmSbarinfoFileHead1098Slice.UnpackCoordinate(packed);
        Assert.Equal(42, value);
        Assert.True(rel);
    }

    [Fact]
    public void AddCoordinate_PreservesRelativeCenterFlag()
    {
        var packed = VmSbarinfoFileHead1098Slice.PackCoordinate(10, relativeCenter: true);
        var adjusted = VmSbarinfoFileHead1098Slice.AddCoordinate(packed, 5);
        var (value, rel) = VmSbarinfoFileHead1098Slice.UnpackCoordinate(adjusted);
        Assert.Equal(15, value);
        Assert.True(rel);
    }

    [Theory]
    [InlineData(VmSbarinfoFileHead1098Slice.DrawOffsetAnchor.Top, 0x1)]
    [InlineData(VmSbarinfoFileHead1098Slice.DrawOffsetAnchor.Center, 0x42)]
    public void DrawOffsetAnchor_MatchesNativeBitmask(VmSbarinfoFileHead1098Slice.DrawOffsetAnchor anchor, int expected)
    {
        Assert.Equal(expected, (int)anchor);
    }

    [Fact]
    public void StatusBarArtFlashOffset_MatchesArtiflashMacro()
    {
        Assert.Equal(12, VmSbarinfoFileHead1098Slice.StatusBarArtFlashOffset(6));
    }

    [Fact]
    public void IsValidStatusBarArtIndex_AcceptsNativeRange()
    {
        Assert.True(VmSbarinfoFileHead1098Slice.IsValidStatusBarArtIndex(4));
        Assert.False(VmSbarinfoFileHead1098Slice.IsValidStatusBarArtIndex(99));
    }

    [Fact]
    public void CombineOffsetAnchors_MergesVerticalAndHorizontal()
    {
        var combined = VmSbarinfoFileHead1098Slice.CombineOffsetAnchors(
            VmSbarinfoFileHead1098Slice.DrawOffsetAnchor.Bottom,
            VmSbarinfoFileHead1098Slice.DrawOffsetAnchor.HMiddle);
        Assert.Equal(VmSbarinfoFileHead1098Slice.DrawOffsetAnchor.CenterBottom, combined);
    }

    [Fact]
    public void GetCoordinatesLoopRequiresComma_OnlyAfterFirstAxis()
    {
        Assert.False(VmSbarinfoFileHead1098Slice.GetCoordinatesLoopRequiresComma(0));
        Assert.True(VmSbarinfoFileHead1098Slice.GetCoordinatesLoopRequiresComma(1));
    }
}
