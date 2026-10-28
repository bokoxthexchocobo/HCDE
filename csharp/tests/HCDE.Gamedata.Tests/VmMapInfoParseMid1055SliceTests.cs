namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMid1055SliceTests
{
    [Fact]
    public void TryResolveOptionalFloatCompat_AcceptsLegacyNumber()
    {
        var warned = false;
        Assert.True(VmMapInfoParseMid1055Slice.TryResolveOptionalFloatCompat(
            standardCheckFloatSucceeded: false,
            VmMapInfoParseMid1055Slice.FmtNew,
            skySpeedNoCommaCompatActive: true,
            scannerHasFloat: false,
            scannerFloat: 0,
            scannerHasNumber: true,
            scannerNumber: 42,
            ref warned,
            "skyspeed",
            out var value,
            out var msg));
        Assert.Equal(42, value);
        Assert.NotNull(msg);
    }

    [Fact]
    public void CountSkipToNextTokens_CountsCommaSeparatedValues()
    {
        Assert.Equal(3, VmMapInfoParseMid1055Slice.CountSkipToNextTokens(true, new[] { true, true, false }));
    }

    [Fact]
    public void ShouldScriptErrorUnexpectedEof_OnlyNewFormatAtEnd()
    {
        Assert.True(VmMapInfoParseMid1055Slice.ShouldScriptErrorUnexpectedEof(
            VmMapInfoParseMid1055Slice.FmtNew, scannerAtEnd: true));
        Assert.False(VmMapInfoParseMid1055Slice.ShouldScriptErrorUnexpectedEof(
            VmMapInfoParseMid1055Slice.FmtOld, scannerAtEnd: true));
    }

    [Fact]
    public void TryParseDollarLookupPrefix_StripsSigil()
    {
        Assert.True(VmMapInfoParseMid1055Slice.TryParseDollarLookupPrefix("$TITLE", out var name));
        Assert.Equal("TITLE", name);
    }

    [Fact]
    public void TrimTrailingNewlineFromMultiline_RemovesFinalNewline()
    {
        Assert.Equal("a\nb", VmMapInfoParseMid1055Slice.TrimTrailingNewlineFromMultiline("a\nb\n"));
    }

    [Fact]
    public void ShouldStopMultilineOnTrailingCommaCompat_DetectsCloseBraceAfterComma()
    {
        var reported = false;
        Assert.True(VmMapInfoParseMid1055Slice.ShouldStopMultilineOnTrailingCommaCompat(
            sawComma: true,
            trailingCommaCompatActive: true,
            nextTokenIsCloseBrace: true,
            ref reported));
        Assert.True(reported);
    }

    [Fact]
    public void TokenIsLookupKeyword_IsCaseInsensitive()
    {
        Assert.True(VmMapInfoParseMid1055Slice.TokenIsLookupKeyword("LOOKUP"));
    }

    [Fact]
    public void AssignMustGetString_CoalescesNull()
    {
        var field = "old";
        VmMapInfoParseMid1055Slice.AssignMustGetString(ref field, null);
        Assert.Equal(string.Empty, field);
    }
}
