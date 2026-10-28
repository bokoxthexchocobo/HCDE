namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawNumber1081SliceTests
{
    [Fact]
    public void ClassifyDrawNumberParseKeyword_MapsFillZerosAndRejectsUnknown()
    {
        var (ok, kw) = VmSbarinfoCommandDrawNumber1081Slice.ClassifyDrawNumberParseKeyword("fillzeros");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawNumber1081Slice.DrawNumberParseKeyword.FillZeros, kw);
        var (bad, unknown) = VmSbarinfoCommandDrawNumber1081Slice.ClassifyDrawNumberParseKeyword("notaflag");
        Assert.False(bad);
        Assert.Equal(VmSbarinfoCommandDrawNumber1081Slice.DrawNumberParseKeyword.Unknown, unknown);
    }

    [Fact]
    public void ResolveDrawNumberFlagListSeparator_MatchesNativePipeComma()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1081Slice.DrawNumberFlagListSeparator.CommaRequired,
            VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberFlagListSeparator(sawPipeSeparator: false));
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1081Slice.DrawNumberFlagListSeparator.PipeContinues,
            VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberFlagListSeparator(sawPipeSeparator: true));
    }

    [Fact]
    public void ApplyDrawNumberShadowFlag_UsesOptionalOffsets()
    {
        var (on, x, y) = VmSbarinfoCommandDrawNumber1081Slice.ApplyDrawNumberShadowFlag(
            sawDrawShadowKeyword: true,
            hasParenthesizedOffsets: true,
            parsedShadowX: 3,
            parsedShadowY: 4,
            priorShadowX: 1,
            priorShadowY: 2);
        Assert.True(on);
        Assert.Equal(3, x);
        Assert.Equal(4, y);
    }

    [Fact]
    public void ResolveDrawNumberPrefixWhenConstant_DisablesPrefixAndConcatenates()
    {
        var (usePrefix, padding) = VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberPrefixWhenConstant(
            requestedUsePrefix: true,
            valueIsConstant: true,
            constantPrefix: "HP:",
            paddingSuffix: " ");
        Assert.False(usePrefix);
        Assert.Equal("HP: ", padding);
    }

    [Fact]
    public void ShouldParseDrawNumberLowHighColoring_MatchesNativeCommaGates()
    {
        Assert.False(VmSbarinfoCommandDrawNumber1081Slice.ShouldParseDrawNumberLowHighColoring(
            sawCommaAfterCoordinates: false,
            parsedFontSpacingInt: false,
            sawCommaAfterSpacingSlot: false));
        Assert.True(VmSbarinfoCommandDrawNumber1081Slice.ShouldParseDrawNumberLowHighColoring(
            sawCommaAfterCoordinates: true,
            parsedFontSpacingInt: false,
            sawCommaAfterSpacingSlot: false));
        Assert.False(VmSbarinfoCommandDrawNumber1081Slice.ShouldParseDrawNumberLowHighColoring(
            sawCommaAfterCoordinates: true,
            parsedFontSpacingInt: true,
            sawCommaAfterSpacingSlot: false));
    }

    [Fact]
    public void ResolveDrawNumberParseClosingInterpolation_OverridesHealthArmorFromScript()
    {
        var speed = VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberParseClosingInterpolation(
            VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Health,
            scriptInterpolateHealth: true,
            scriptHealthSpeed: 8,
            scriptInterpolateArmor: false,
            scriptArmorSpeed: 0,
            parseFlagSpeed: 2);
        Assert.Equal(8, speed);
    }

    [Fact]
    public void ClampDrawNumberHealthForDisplay_ZeroesNegativesWhenCapped()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawNumber1081Slice.ClampDrawNumberHealthForDisplay(-5, lowerHealthCap: true));
        Assert.Equal(-5, VmSbarinfoCommandDrawNumber1081Slice.ClampDrawNumberHealthForDisplay(-5, lowerHealthCap: false));
    }

    [Fact]
    public void ResolveDrawNumberPrefixTickCache_InvalidatesWhenPrefixEnabled()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawString1078Slice.InvalidStringCache,
            VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberPrefixTickCache(usePrefix: true, priorCache: 42));
    }

    [Fact]
    public void ResolveDrawNumberTickWhenAmmoMissing_ClearsStringAndReturns()
    {
        var (abort, text) = VmSbarinfoCommandDrawNumber1081Slice.ResolveDrawNumberTickWhenAmmoMissing(
            ammoIsNull: true,
            priorDisplayString: "keep");
        Assert.True(abort);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void ResolveInventoryDrawNumberAmount_ReturnsZeroWhenMissing()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawNumber1081Slice.ResolveInventoryDrawNumberAmount(itemFound: false, amount: 99));
    }
}
