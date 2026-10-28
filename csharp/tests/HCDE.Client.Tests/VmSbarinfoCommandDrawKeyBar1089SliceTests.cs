namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawKeyBar1089SliceTests
{
    [Fact]
    public void TryClassifyKeyBarParseFlagKeyword_MapsReverseRows()
    {
        var (ok, flag) = VmSbarinfoCommandDrawKeyBar1089Slice.TryClassifyKeyBarParseFlagKeyword("reverserows");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawKeyBar1089Slice.KeyBarParseFlag.ReverseRows, flag);
    }

    [Fact]
    public void ResolveKeyBarFlagListContinuation_EndsOnComma()
    {
        var (cont, comma) = VmSbarinfoCommandDrawKeyBar1089Slice.ResolveKeyBarFlagListContinuation(
            nextTokenIsPipe: false);
        Assert.False(cont);
        Assert.True(comma);
    }

    [Fact]
    public void ResolveKeyBarIconSizeFromParse_AutoUsesNegativeOne()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawKeyBar1088Slice.UnspecifiedIconSize,
            VmSbarinfoCommandDrawKeyBar1089Slice.ResolveKeyBarIconSizeFromParse(
                autoToken: true,
                intConst: 48));
    }

    [Fact]
    public void ResolveKeyBarOptionalLayoutParse_ReadsRowWhenTwoCommas()
    {
        var (keyOffset, rowLayout) = VmSbarinfoCommandDrawKeyBar1089Slice.ResolveKeyBarOptionalLayoutParse(
            commaAfterBaseCoordinates: true,
            commaAfterKeyOffset: true);
        Assert.True(keyOffset);
        Assert.True(rowLayout);
    }

    [Fact]
    public void CreateDrawBarCtorDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoCommandDrawBar1089Slice.CreateDrawBarCtorDefaults();
        Assert.Equal(VmSbarinfoCommandDrawBar1089Slice.MissingImageIndex, d.Foreground);
        Assert.Equal(VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType.Health, d.Type);
    }

    [Fact]
    public void InvertNormalizedDrawValueForBorderDraw_FlipsWhenBorderNonZero()
    {
        Assert.Equal(0.7, VmSbarinfoCommandDrawBar1089Slice.InvertNormalizedDrawValueForBorderDraw(
            border: 2,
            drawValue: 0.3),
            5);
    }

    [Fact]
    public void ComputeDrawBarClipFillRemaining_ScalesByValue()
    {
        Assert.Equal(75, VmSbarinfoCommandDrawBar1089Slice.ComputeDrawBarClipFillRemaining(
            sizeOfImage: 100,
            normalizedValue: 0.25));
    }

    [Fact]
    public void ResolveDrawBarClipComponentIndex_VerticalReverseUsesIndexThree()
    {
        Assert.Equal(
            3,
            VmSbarinfoCommandDrawBar1089Slice.ResolveDrawBarClipComponentIndex(
                horizontal: false,
                reverse: true));
    }

    [Fact]
    public void TryClassifyDrawBarStatKeyword_HealthUsesComparator()
    {
        var (ok, type, cmp) = VmSbarinfoCommandDrawBar1089Slice.TryClassifyDrawBarStatKeyword("health");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType.Health, type);
        Assert.True(cmp);
    }

    [Fact]
    public void ShouldReplaceDrawBarAmmoClass_WhenNotAmmoKind()
    {
        Assert.True(VmSbarinfoCommandDrawBar1089Slice.ShouldReplaceDrawBarAmmoClass(
            classFound: true,
            isAmmoKind: false));
    }
}
