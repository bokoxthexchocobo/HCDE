namespace HCDE.Client.Tests;

public class VmVFontGetFont1072SliceTests
{
    [Fact]
    public void BuildFolderFontPath_MatchesNativePattern()
    {
        Assert.Equal("fonts/MyFont/", VmVFontGetFont1072Slice.BuildFolderFontPath("MyFont"));
    }

    [Fact]
    public void ResolveFontLumpLookupName_FallsBackToCanonicalName()
    {
        Assert.Equal("SmallFont", VmVFontGetFont1072Slice.ResolveFontLumpLookupName(null, "SmallFont"));
        Assert.Equal("LUMP", VmVFontGetFont1072Slice.ResolveFontLumpLookupName("LUMP", "SmallFont"));
    }
    [Fact]
    public void ResolveCanonicalFontLookupName_MapsLegacyAliases()
    {
        Assert.Equal("BigFont", VmVFontGetFont1072Slice.ResolveCanonicalFontLookupName("DBIGFONT"));
        Assert.Equal("ConsoleFont", VmVFontGetFont1072Slice.ResolveCanonicalFontLookupName("confont"));
    }

    [Fact]
    public void ShouldPreferSingleLumpOverFolder_RequiresContainerOrdering()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldPreferSingleLumpOverFolder(1, lumpContainer: 5, folderContainer: 3));
        Assert.False(VmVFontGetFont1072Slice.ShouldPreferSingleLumpOverFolder(-1, 5, 3));
    }

    [Fact]
    public void IsRecognizedSingleLumpFontHeader_AcceptsFonMagic()
    {
        Assert.True(VmVFontGetFont1072Slice.IsRecognizedSingleLumpFontHeader(VmVFontGetFont1072Slice.FontLumpMagicFon0));
        Assert.True(VmVFontGetFont1072Slice.IsRecognizedSingleLumpFontHeader(VmVFontGetFont1072Slice.FontLumpMagicCompressed));
    }

    [Fact]
    public void ShouldCreatePicFontFromTexture_RequiresSourceLumpOrdering()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldCreatePicFontFromTexture(true, 4, folderContainer: 2));
        Assert.False(VmVFontGetFont1072Slice.ShouldCreatePicFontFromTexture(true, 1, folderContainer: 4));
    }

    [Fact]
    public void ShouldInstantiateFolderFont_WhenEntriesExist()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldInstantiateFolderFont(3));
        Assert.False(VmVFontGetFont1072Slice.ShouldInstantiateFolderFont(0));
    }

    [Fact]
    public void ResolveFolderContainerWhenPresent_ReturnsSentinelWhenMissing()
    {
        Assert.Equal(-1, VmVFontGetFont1072Slice.ResolveFolderContainerWhenPresent(false, 9));
        Assert.Equal(9, VmVFontGetFont1072Slice.ResolveFolderContainerWhenPresent(true, 9));
    }

    [Fact]
    public void DefaultLogPaletteEntry_MatchesMacro()
    {
        var (r, g, b) = VmVFontGetFont1072Slice.DefaultLogPaletteEntry();
        Assert.Equal(223, r);
        Assert.Equal(223, g);
        Assert.Equal(223, b);
    }

    [Fact]
    public void ShouldTryBigUpperFallback_OnlyForAlias()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldTryBigUpperFallback("BIGUPPER"));
        Assert.False(VmVFontGetFont1072Slice.ShouldTryBigUpperFallback("BIGFONT"));
    }

    [Fact]
    public void ShouldAbortFontLookup_WhenNameNull()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldAbortFontLookup(null));
        Assert.False(VmVFontGetFont1072Slice.ShouldAbortFontLookup("SmallFont"));
    }

    [Fact]
    public void ShouldLoadTranslationsAfterFontCreate_WhenAlreadyLoaded()
    {
        Assert.True(VmVFontGetFont1072Slice.ShouldLoadTranslationsAfterFontCreate(
            new VmVFontGetFont1072Slice.FontCreateFollowUp(CreatedFont: true, TranslationsLoaded: true)));
        Assert.False(VmVFontGetFont1072Slice.ShouldLoadTranslationsAfterFontCreate(
            new VmVFontGetFont1072Slice.FontCreateFollowUp(true, false)));
    }

    [Fact]
    public void ClassifyLegacyFontAlias_RecognizesConfont()
    {
        Assert.Equal(VmVFontGetFont1072Slice.LegacyFontAliasKind.ConsoleFont,
            VmVFontGetFont1072Slice.ClassifyLegacyFontAlias("CONFONT"));
    }
}
