namespace HCDE.Client.Tests;

public class VmVFontTextColo1060SliceTests
{
    [Fact]
    public void ValidateColorBlockParmLengths_RequiresNormalRanges()
    {
        Assert.Equal(
            VmVFontTextColo1060Slice.TextColoBlockValidationError.MissingNormalRange,
            VmVFontTextColo1060Slice.ValidateColorBlockParmLengths(0, "Red"));
    }

    [Fact]
    public void ValidateColorBlockParmLengths_UntranslatedMustStayEmpty()
    {
        Assert.Equal(
            VmVFontTextColo1060Slice.TextColoBlockValidationError.UntranslatedMustBeUndefined,
            VmVFontTextColo1060Slice.ValidateColorBlockParmLengths(2, "Untranslated"));
    }

    [Fact]
    public void ShouldSynthesizeDefaultConsoleTranslation_SkipsUntranslated()
    {
        Assert.False(VmVFontTextColo1060Slice.ShouldSynthesizeDefaultConsoleTranslation(0, "Untranslated"));
        Assert.True(VmVFontTextColo1060Slice.ShouldSynthesizeDefaultConsoleTranslation(0, "Green"));
    }

    [Fact]
    public void CreateDefaultConsoleTranslation_MatchesNativeWhiteFill()
    {
        var parm = VmVFontTextColo1060Slice.CreateDefaultConsoleTranslation();
        Assert.Equal(0, parm.RangeStart);
        Assert.Equal(256, parm.RangeEnd);
        Assert.Equal((byte)255, parm.EndR);
    }

    [Fact]
    public void ShouldRegisterNewColorName_WhenNoDuplicate()
    {
        Assert.True(VmVFontTextColo1060Slice.ShouldRegisterNewColorName(duplicateScanIndex: 3, existingCount: 3));
        Assert.False(VmVFontTextColo1060Slice.ShouldRegisterNewColorName(1, 3));
    }

    [Fact]
    public void AllocateNextTranslationIndex_ReusesAssigned()
    {
        var next = 2;
        Assert.Equal(5, VmVFontTextColo1060Slice.AllocateNextTranslationIndex(5, ref next));
        Assert.Equal(2, next);
        Assert.Equal(2, VmVFontTextColo1060Slice.AllocateNextTranslationIndex(-1, ref next));
        Assert.Equal(3, next);
    }

    [Fact]
    public void BinarySearchTranslationNumber_FindsNamedColor()
    {
        var table = new[]
        {
            new VmVFontTextColo1060Slice.TranslationMapEntry(10, 0),
            new VmVFontTextColo1060Slice.TranslationMapEntry(20, 1),
            new VmVFontTextColo1060Slice.TranslationMapEntry(30, 2),
        };
        Assert.Equal(1, VmVFontTextColo1060Slice.BinarySearchTranslationNumber(table, 20));
        Assert.Equal(VmVFontTextColo1060Slice.UntranslatedColorRange,
            VmVFontTextColo1060Slice.BinarySearchTranslationNumber(table, 99));
    }
}
