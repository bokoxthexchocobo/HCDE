namespace HCDE.Client.Tests;

public class VmVFontFontDefsTextColo1059SliceTests
{
    [Fact]
    public void ComputeLumpMapGlyphSpan_FindsFirstAndCount()
    {
        var slots = new bool[256];
        slots[40] = true;
        slots[50] = true;
        var span = VmVFontFontDefsTextColo1059Slice.ComputeLumpMapGlyphSpan(slots);
        Assert.Equal(40, span.First);
        Assert.Equal(11, span.Count);
    }

    [Fact]
    public void ComputeLumpMapGlyphSpan_EmptyReturnsZeroCount()
    {
        var span = VmVFontFontDefsTextColo1059Slice.ComputeLumpMapGlyphSpan(new bool[256]);
        Assert.Equal(0, span.Count);
        Assert.False(VmVFontFontDefsTextColo1059Slice.ShouldInstantiateLumpMapFont(span.Count));
    }

    [Fact]
    public void RejectsAliasForUntranslated_WhenExtraNamesPresent()
    {
        Assert.True(VmVFontFontDefsTextColo1059Slice.RejectsAliasForUntranslated(
            "untranslated", new[] { "alias" }));
    }

    [Fact]
    public void TrySwitchToConsoleRange_RejectsDuplicateConsoleBlock()
    {
        Assert.False(VmVFontFontDefsTextColo1059Slice.TrySwitchToConsoleRange(1, out _, out var dup));
        Assert.True(dup);
    }

    [Fact]
    public void ValidateExplicitColorRange_EnforcesNativeOrdering()
    {
        Assert.Equal(
            VmVFontFontDefsTextColo1059Slice.ColorRangeValidationError.FirstRangeMustStartAtZero,
            VmVFontFontDefsTextColo1059Slice.ValidateExplicitColorRange(-1, 1, 16));
        Assert.Equal(
            VmVFontFontDefsTextColo1059Slice.ColorRangeValidationError.OverlapsPrevious,
            VmVFontFontDefsTextColo1059Slice.ValidateExplicitColorRange(32, 32, 64));
        Assert.Equal(
            VmVFontFontDefsTextColo1059Slice.ColorRangeValidationError.EndNotAfterStart,
            VmVFontFontDefsTextColo1059Slice.ValidateExplicitColorRange(0, 10, 10));
    }

    [Fact]
    public void TryBuildImplicitColorRange_MatchesNativeTail()
    {
        Assert.True(VmVFontFontDefsTextColo1059Slice.TryBuildImplicitColorRange(200, out var span, out var err));
        Assert.Equal(VmVFontFontDefsTextColo1059Slice.ColorRangeValidationError.None, err);
        Assert.Equal(201, span.Start);
        Assert.Equal(256, span.End);
    }

    [Fact]
    public void SplitRgb_MatchesNativeParts()
    {
        var (r, g, b) = VmVFontFontDefsTextColo1059Slice.SplitRgb(0x00ff8040);
        Assert.Equal(0x40, r);
        Assert.Equal(0x80, g);
        Assert.Equal(0xff, b);
    }
}
