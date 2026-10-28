namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppRenderStylesTests
{
    [Fact]
    public void TryParseEntry_TruncatesName()
    {
        Assert.True(VmDehackedDehSuppRenderStyles.TryParseEntry(
            1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", out var entry));
        Assert.Equal(VmDehackedDehSuppRenderStyles.MaxNameLength, entry.Name.Length);
    }

    [Fact]
    public void ParseRenderStylesSection_ReadsRows()
    {
        var list = VmDehackedDehSuppRenderStyles.ParseRenderStylesSection([(0, "Normal"), (1, "Add")]);
        Assert.Equal(2, list.Count);
        Assert.Equal("Add", list[1].Name);
    }
}
