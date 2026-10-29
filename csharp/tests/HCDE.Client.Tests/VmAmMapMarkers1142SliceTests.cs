namespace HCDE.Client.Tests;

public class VmAmMapMarkers1142SliceTests
{
    [Fact]
    public void CheatBox_IsAUnitSquareAtTheRemainingYaw()
    {
        Assert.True(VmAmMapMarkers1142Slice.DrawCheatBox(3));
        Assert.True(VmAmMapMarkers1142Slice.DrawCheatBox(6));
        Assert.False(VmAmMapMarkers1142Slice.DrawCheatBox(4));
        Assert.Equal(0d, VmAmMapMarkers1142Slice.CheatBoxAngle(30, 30));
        Assert.Equal(100d, VmAmMapMarkers1142Slice.CheatBoxAngle(40, -60));
        var box = VmAmMapMarkers1142Slice.CheatBox();
        Assert.Equal(4, box.Count);
        Assert.Equal((-1d, -1d), (box[0].Ax, box[0].Ay));
        Assert.Equal((-1d, -1d), (box[3].Bx, box[3].By));
    }

    [Fact]
    public void PlaceMarker_FlipsANegativeScaleAndTruncatesTheFramePoint()
    {
        var missing = VmAmMapMarkers1142Slice.PlaceMarker(false, false, 0, 0, 1, 1, 8, 8, 1, 1, 0, 0, 0, 1, 0, 0, 100);
        Assert.False(missing.Draw);
        var placed = VmAmMapMarkers1142Slice.PlaceMarker(true, false, 10.9, 4, -2, 3, 8, 6, 2, 4, -3, 0, 0, 1, 5, 7, 100);
        Assert.True(placed.Flip);
        Assert.Equal(2d, placed.ScaleX);
        Assert.Equal(15d, placed.ScreenX);
        Assert.Equal(100d, placed.ScreenY);
        Assert.Equal(32d, placed.DestWidth);
        Assert.Equal(72d, placed.DestHeight);
    }

    [Fact]
    public void Marks_UseTexturesForTheDefaultFontAndDigitsOtherwise()
    {
        Assert.False(VmAmMapMarkers1142Slice.UseCustomMarkFont("ammnumx"));
        Assert.True(VmAmMapMarkers1142Slice.UseCustomMarkFont("BigFont"));
        Assert.Equal(VmAmMapMarkers1142Slice.MarkDraw.Skip, VmAmMapMarkers1142Slice.MarkStyle(-1, true));
        Assert.Equal(VmAmMapMarkers1142Slice.MarkDraw.Texture, VmAmMapMarkers1142Slice.MarkStyle(4, false));
        Assert.Equal(VmAmMapMarkers1142Slice.MarkDraw.Text, VmAmMapMarkers1142Slice.MarkStyle(4, true));
        Assert.Equal("7", VmAmMapMarkers1142Slice.MarkDigit(7));
        Assert.Equal(VmAmMapMarkers1142Slice.MarkCount, 10);
        var turned = VmAmMapMarkers1142Slice.MarkPoint(1, 2, true, (x, y) => (x + 3, y));
        Assert.Equal((4d, 2d), turned);
        Assert.Equal((9, 80), VmAmMapMarkers1142Slice.MarkTextOrigin(9.2, 20, 0, 0, 1, 100));
    }

    [Fact]
    public void AuthorMarkers_SkipDormantActorsAndReadTheFirstFlipBit()
    {
        Assert.True(VmAmMapMarkers1142Slice.SkipDormantMarker(VmAmMapMarkers1142Slice.Dormant));
        Assert.False(VmAmMapMarkers1142Slice.SkipDormantMarker(0));
        Assert.Equal(VmAmMapMarkers1142Slice.AuthorTexture.Picture, VmAmMapMarkers1142Slice.AuthorSource(true, VmAmMapMarkers1142Slice.NoRotations, 0, 1));
        Assert.Equal(VmAmMapMarkers1142Slice.AuthorTexture.RotationFrame, VmAmMapMarkers1142Slice.AuthorSource(true, 4, 0, 1));
        Assert.Equal(VmAmMapMarkers1142Slice.AuthorTexture.Missing, VmAmMapMarkers1142Slice.AuthorSource(false, 0, 3, 3));
        Assert.Equal(VmAmMapMarkers1142Slice.AuthorTexture.SpriteFrame, VmAmMapMarkers1142Slice.AuthorSource(false, 0, 1, 3));
        Assert.False(VmAmMapMarkers1142Slice.AuthorFlip(VmAmMapMarkers1142Slice.AuthorTexture.Picture, 1));
        Assert.True(VmAmMapMarkers1142Slice.AuthorFlip(VmAmMapMarkers1142Slice.AuthorTexture.SpriteFrame, 1));
        Assert.False(VmAmMapMarkers1142Slice.AuthorFlip(VmAmMapMarkers1142Slice.AuthorTexture.RotationFrame, 2));
    }
}
