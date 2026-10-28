namespace HCDE.Client.Tests;

public class VmVFontParseInit1062SliceTests
{
    [Fact]
    public void AdvanceGapFillCursor_MatchesNativeIncrement()
    {
        var (low, high) = VmVFontParseInit1062Slice.AdvanceGapFillCursor(lowIndex: 10, highIndex: 20);
        Assert.Equal(20, low);
        Assert.Equal(21, high);
    }

    [Fact]
    public void ShouldUseDefaultLogColor_WhenOutOfRange()
    {
        Assert.True(VmVFontParseInit1062Slice.ShouldUseDefaultLogColor(range: 5, translationColorCount: 5));
        Assert.False(VmVFontParseInit1062Slice.ShouldUseDefaultLogColor(4, 5));
    }

    [Fact]
    public void ResolveStandardColorLetter_AcceptsCase()
    {
        Assert.Equal(0, VmVFontParseInit1062Slice.ResolveStandardColorLetter('A', numTextColors: 20));
        Assert.Equal(0, VmVFontParseInit1062Slice.ResolveStandardColorLetter('a', numTextColors: 20));
        Assert.Equal(VmVFontParseInit1062Slice.ColorUndefined,
            VmVFontParseInit1062Slice.ResolveStandardColorLetter('z', numTextColors: 5));
    }

    [Fact]
    public void ClassifyFontColorEscape_RecognizesNativeLeaders()
    {
        Assert.Equal(VmVFontParseInit1062Slice.FontColorEscapeKind.NamedBracket,
            VmVFontParseInit1062Slice.ClassifyFontColorEscape('['));
        Assert.Equal(VmVFontParseInit1062Slice.FontColorEscapeKind.TeamChatAlias,
            VmVFontParseInit1062Slice.ClassifyFontColorEscape('!'));
    }

    [Fact]
    public void TryParseNamedColorToken_ExtractsBracketName()
    {
        var input = System.Text.Encoding.Latin1.GetBytes("[Gold]rest");
        Assert.True(VmVFontParseInit1062Slice.TryParseNamedColorToken(input, out var rem, out var name));
        Assert.Equal("Gold", name);
        Assert.Equal("rest", System.Text.Encoding.Latin1.GetString(rem));
    }

    [Fact]
    public void PatchLuminosityTranslationIndex_ReplacesHighBits()
    {
        var existing = unchecked((int)(0xBEEF0000 | 0x1234));
        var patched = VmVFontParseInit1062Slice.PatchLuminosityTranslationIndex(
            existing, VmVFontParseInit1062Slice.SmallFontDefaultTranslationTableIndex());
        Assert.Equal(VmVFontParseInit1062Slice.SmallFontDefaultTranslationTableIndex() << 16, patched & 0x3fff0000);
        Assert.Equal(0x1234, patched & 0xffff);
    }

    [Fact]
    public void RequiresConsoleHexFont_WhenLumpMissing()
    {
        Assert.True(VmVFontParseInit1062Slice.RequiresConsoleHexFont(-1));
        Assert.False(VmVFontParseInit1062Slice.RequiresConsoleHexFont(3));
    }
}
