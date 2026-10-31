namespace HCDE.Client.Tests;

public class VmAmMapStaticInit1127SliceTests
{
    [Fact]
    public void ArrowLumpsToLoad_SkipsEmptyOptionalNamesAndAlwaysLoadsKeys()
    {
        var lumps = VmAmMapStaticInit1127Slice.ArrowLumpsToLoad("", "cheatmap", "key", "easy");
        Assert.Equal(new[] { "cheatmap", "key", "easy" }, lumps);
        Assert.True(VmAmMapStaticInit1127Slice.MissingMapArrowIsFatal(0));
        Assert.False(VmAmMapStaticInit1127Slice.MissingMapArrowIsFatal(2));
    }

    [Fact]
    public void MarkTextureNames_AreAmmnumZeroThroughNine()
    {
        var names = VmAmMapStaticInit1127Slice.MarkTextureNames();
        Assert.Equal(10, names.Length);
        Assert.Equal("AMMNUM0", names[0]);
        Assert.Equal("AMMNUM9", names[9]);
    }

    [Fact]
    public void PanIncrement_IsFourPixelsPerTic()
    {
        var defaults = VmAmMapStaticInit1127Slice.CreateWindowDefaults();
        Assert.Equal(4, defaults.PanIncrement);
        Assert.Equal(0.2, defaults.ScaleMapToFrame);
        Assert.Equal(0, defaults.MarkPointNumber);
        Assert.Equal(0, defaults.MapXStart);
    }

    [Fact]
    public void FrameMapScale_TruncatesWindowCoordinates()
    {
        Assert.Equal(50, VmAmMapStaticInit1127Slice.FrameToMap(10, 5));
        Assert.Equal(2, VmAmMapStaticInit1127Slice.MapToFrame(10, 0.2));
        Assert.Equal(12, VmAmMapStaticInit1127Slice.MapXToFrame(10, 4, 2));
        Assert.Equal(88, VmAmMapStaticInit1127Slice.MapYToFrame(10, 4, 2, 100));
    }
}
