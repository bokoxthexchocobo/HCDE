namespace HCDE.Client.Tests;

public class VmSbarinfoDrawString1071SliceTests
{
    [Fact]
    public void ResolveBoldTranslationRange_MatchesNativeFallback()
    {
        Assert.Equal(7, VmSbarinfoDrawString1071Slice.ResolveBoldTranslationRange(0, numTextColors: 8));
        Assert.Equal(2, VmSbarinfoDrawString1071Slice.ResolveBoldTranslationRange(3, 8));
    }

    [Fact]
    public void AdvanceDrawStringForSpace_UsesSpacingCharacterWhenSet()
    {
        Assert.Equal(20, VmSbarinfoDrawString1071Slice.AdvanceDrawStringForSpace(10, ' ', 8, 10));
        Assert.Equal(18, VmSbarinfoDrawString1071Slice.AdvanceDrawStringForSpace(10, '\0', 8, 10));
    }

    [Fact]
    public void ApplyMonospaceAlignmentX_CentersWithinSpacingCell()
    {
        var rx = VmSbarinfoDrawString1071Slice.ApplyMonospaceAlignmentX(
            5, glyphWidth: 8, spacingSize: 20,
            VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Center);
        Assert.Equal(11, rx);
    }

    [Fact]
    public void ComputeDrawStringShadowAlpha_UsesHereticShadowFactor()
    {
        Assert.Equal(0.5 * VmSbarinfoDrawString1071Slice.HereticShadowAlphaFactor,
            VmSbarinfoDrawString1071Slice.ComputeDrawStringShadowAlpha(0.5), 6);
    }

    [Fact]
    public void AdvanceDrawStringCursorAfterGlyph_DiffersForMonospace()
    {
        var prop = VmSbarinfoDrawString1071Slice.AdvanceDrawStringCursorAfterGlyph(
            10, glyphWidth: 6, spacing: 1, displayLeftOffset: 2, proportionalMode: true, spacingCharacterWidth: 12);
        var mono = VmSbarinfoDrawString1071Slice.AdvanceDrawStringCursorAfterGlyph(
            10, 6, 1, 2, proportionalMode: false, spacingCharacterWidth: 12);
        Assert.True(mono > prop);
    }

    [Fact]
    public void ShouldUseRavenExtraPlayerTranslation_DetectsRavenMask()
    {
        Assert.True(VmSbarinfoDrawString1071Slice.ShouldUseRavenExtraPlayerTranslation(gameType: 6));
        Assert.False(VmSbarinfoDrawString1071Slice.ShouldUseRavenExtraPlayerTranslation(1));
    }

    [Fact]
    public void ResolveTranslationIndex_GatesOnTranslateFlag()
    {
        Assert.Equal(0, VmSbarinfoDrawString1071Slice.ResolveTranslationIndex(false, 5));
        Assert.Equal(5, VmSbarinfoDrawString1071Slice.ResolveTranslationIndex(true, 5));
    }

    [Fact]
    public void AnchorNegativeFullscreenGlyph_AddsViewSize()
    {
        var (rx, _) = VmSbarinfoDrawString1071Slice.AnchorNegativeFullscreenGlyph(
            -10, 0, anchorFromRight: true, anchorFromBottom: false, 640, 480);
        Assert.Equal(630, rx);
    }

    [Fact]
    public void IsTextColorEscape_RecognizesNativeEscape()
    {
        Assert.True(VmSbarinfoDrawString1071Slice.IsTextColorEscape(0x1c));
    }

    [Fact]
    public void SelectAmmoTypeFieldIndex_MapsSlotOneAndTwo()
    {
        Assert.Equal(1, VmSbarinfoDrawString1071Slice.SelectAmmoTypeFieldIndex(1));
        Assert.Equal(2, VmSbarinfoDrawString1071Slice.SelectAmmoTypeFieldIndex(2));
    }
}
