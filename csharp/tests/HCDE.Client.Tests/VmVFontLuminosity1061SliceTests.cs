namespace HCDE.Client.Tests;

public class VmVFontLuminosity1061SliceTests
{
    [Fact]
    public void ShouldBuildLuminosityRemap_SkipsUntranslatedTable()
    {
        Assert.False(VmVFontLuminosity1061Slice.ShouldBuildLuminosityRemap(
            VmVFontLuminosity1061Slice.UntranslatedTableIndex));
        Assert.True(VmVFontLuminosity1061Slice.ShouldBuildLuminosityRemap(0));
    }

    [Fact]
    public void InterpolateTranslationColor_MatchesNativeLinearBlend()
    {
        var parm = new VmVFontLuminosity1061Slice.TranslationParmSlice(
            0, 256, 0, 0, 0, 255, 255, 255);
        var (r, _, _) = VmVFontLuminosity1061Slice.InterpolateTranslationColor(128, parm);
        Assert.InRange(r, (byte)120, (byte)136);
    }

    [Fact]
    public void ShouldAdvanceTranslationParm_WhenRangesGap()
    {
        var current = new VmVFontLuminosity1061Slice.TranslationParmSlice(0, 10, 0, 0, 0, 1, 1, 1);
        var next = new VmVFontLuminosity1061Slice.TranslationParmSlice(20, 30, 0, 0, 0, 1, 1, 1);
        Assert.True(VmVFontLuminosity1061Slice.ShouldAdvanceTranslationParm(current, next));
    }

    [Fact]
    public void ComputeLuminanceRemapIndex_ClampsToByte()
    {
        Assert.Equal(0, VmVFontLuminosity1061Slice.ComputeLuminanceRemapIndex(0, 10, 20));
        Assert.Equal(255, VmVFontLuminosity1061Slice.ComputeLuminanceRemapIndex(100, 0, 99));
    }

    [Fact]
    public void ScaleRgbBelowMin_ScalesWhenAboveZeroMin()
    {
        var scaled = VmVFontLuminosity1061Slice.ScaleRgbBelowMin(100, 50, 25, gray: 50, lumMin: 100);
        Assert.Equal((byte)50, scaled.R);
        Assert.Equal((byte)25, scaled.G);
        Assert.Equal((byte)12, scaled.B);
    }

    [Fact]
    public void ShouldSkipLuminosityApply_WhenRangeTooLarge()
    {
        Assert.True(VmVFontLuminosity1061Slice.ShouldSkipLuminosityApply(colorRange: 40, maxColorRange: 40));
    }

    [Fact]
    public void InterpolateDefaultRemapGap_BlendsBetweenAnchors()
    {
        var mid = VmVFontLuminosity1061Slice.InterpolateDefaultRemapGap(
            0, 0, 0, 100, 100, 100, lowIndex: 10, highIndex: 20, index: 15);
        Assert.Equal((byte)50, mid.R);
        Assert.Equal((byte)50, mid.G);
        Assert.Equal((byte)50, mid.B);
    }
}
