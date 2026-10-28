namespace HCDE.Client.Tests;

public class VmVFontFontDefs1058SliceTests
{
    [Fact]
    public void CreateDefaults_MatchesNativeInitializers()
    {
        var s = VmVFontFontDefs1058Slice.CreateDefaults();
        Assert.Equal(VmVFontFontDefs1058Slice.DefaultStart, s.Start);
        Assert.Equal(VmVFontFontDefs1058Slice.DefaultFirst, s.First);
        Assert.Equal(VmVFontFontDefs1058Slice.DefaultCount, s.Count);
        Assert.Equal(VmVFontFontDefs1058Slice.DefaultSpaceWidth, s.SpaceWidth);
        Assert.Equal(VmVFontFontDefs1058Slice.DefaultCursor, s.Cursor);
        Assert.Equal(VmVFontFontDefs1058Slice.FontDefFormat.None, s.Format);
    }

    [Fact]
    public void FormatConflict_RejectsMixedTemplateAndLumpMap()
    {
        var s = VmVFontFontDefs1058Slice.CreateDefaults();
        Assert.True(VmVFontFontDefs1058Slice.TryApplyTemplateKeyword(s, VmVFontFontDefs1058Slice.FontDefFormat.Template));
        Assert.True(VmVFontFontDefs1058Slice.FormatConflict(s.Format, VmVFontFontDefs1058Slice.FontDefFormat.LumpMap));
        Assert.False(VmVFontFontDefs1058Slice.TryApplyLumpMapKeyword(s));
    }

    [Fact]
    public void TryMarkNoTranslation_OnlyValidCodePoints()
    {
        var s = VmVFontFontDefs1058Slice.CreateDefaults();
        Assert.True(VmVFontFontDefs1058Slice.TryMarkNoTranslation(s, 65));
        Assert.True(s.NoTranslate[65]);
        Assert.False(VmVFontFontDefs1058Slice.TryMarkNoTranslation(s, 300));
    }

    [Fact]
    public void ClampLuminosity_ClampsToByteRange()
    {
        Assert.Equal(0, VmVFontFontDefs1058Slice.ClampLuminosity(-5));
        Assert.Equal(255, VmVFontFontDefs1058Slice.ClampLuminosity(512));
        Assert.Equal(128, VmVFontFontDefs1058Slice.ClampLuminosity(128));
    }

    [Fact]
    public void NormalizeFontGlyphKey_UnescapesBraces()
    {
        Assert.Equal("{", VmVFontFontDefs1058Slice.NormalizeFontGlyphKey("-{"));
        Assert.Equal("}", VmVFontFontDefs1058Slice.NormalizeFontGlyphKey("-}"));
        Assert.Equal("A", VmVFontFontDefs1058Slice.NormalizeFontGlyphKey("A"));
    }

    [Fact]
    public void ShouldEmitMissingTextureMessage_OnlyForNonIwadContainers()
    {
        Assert.True(VmVFontFontDefs1058Slice.ShouldEmitMissingTextureMessage(lumpContainer: 2, iwadNum: 1));
        Assert.False(VmVFontFontDefs1058Slice.ShouldEmitMissingTextureMessage(lumpContainer: 0, iwadNum: 1));
    }
}
