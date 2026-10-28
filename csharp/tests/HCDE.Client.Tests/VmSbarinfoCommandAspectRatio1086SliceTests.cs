namespace HCDE.Client.Tests;

public class VmSbarinfoCommandAspectRatio1086SliceTests
{
    [Fact]
    public void SelectNearestAspectRatioKind_Picks169ForWideScreen()
    {
        var targets = VmSbarinfoCommandAspectRatio1086Slice.CreateNearestRatioTargetTable();
        var kind = VmSbarinfoCommandAspectRatio1086Slice.SelectNearestAspectRatioKind(1.777f, targets);
        Assert.Equal(VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_9, kind);
    }

    [Fact]
    public void CreateDefaultAspectRatioRemapTable_Maps1710To1610()
    {
        var map = VmSbarinfoCommandAspectRatio1086Slice.CreateDefaultAspectRatioRemapTable();
        Assert.Equal((int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_10, map[3]);
    }

    [Fact]
    public void ResolveAspectRatioTickTruthFromNearestFind_UsesRemapTable()
    {
        var map = VmSbarinfoCommandAspectRatio1086Slice.CreateDefaultAspectRatioRemapTable();
        Assert.True(VmSbarinfoCommandAspectRatio1086Slice.ResolveAspectRatioTickTruthFromNearestFind(
            map,
            VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_10,
            VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio17_10));
    }

    [Fact]
    public void ValidateDrawShaderDimensions_RejectsZeroWidth()
    {
        var (wOk, hOk) = VmSbarinfoCommandDrawShader1086Slice.ValidateDrawShaderDimensions(width: 0, height: 2);
        Assert.False(wOk);
        Assert.True(hOk);
    }

    [Fact]
    public void ClassifyDrawShaderDirectionKeyword_MapsVertical()
    {
        var (ok, vertical, unknown) = VmSbarinfoCommandDrawShader1086Slice.ClassifyDrawShaderDirectionKeyword("vertical");
        Assert.True(ok);
        Assert.True(vertical);
        Assert.False(unknown);
    }

    [Fact]
    public void ResolveDrawShaderTextureSlotIndex_EncodesVerticalReverse()
    {
        Assert.Equal(3, VmSbarinfoCommandDrawShader1086Slice.ResolveDrawShaderTextureSlotIndex(vertical: true, reverse: true));
    }

    [Fact]
    public void ApplyDrawShaderReverseKeyword_EnablesReverseAndComma()
    {
        var (reverse, comma) = VmSbarinfoCommandDrawShader1086Slice.ApplyDrawShaderReverseKeyword(sawReverseKeyword: true);
        Assert.True(reverse);
        Assert.True(comma);
    }

    [Fact]
    public void CreateDrawInventoryBarCtorDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoCommandDrawInventoryBar1086Slice.CreateDrawInventoryBarCtorDefaults();
        Assert.Equal(VmSbarinfoCommandDrawInventoryBar1086Slice.InventoryBarStyle.Doom, d.Style);
        Assert.Equal(7, d.Size);
        Assert.Equal(2, d.ShadowX);
    }
}
