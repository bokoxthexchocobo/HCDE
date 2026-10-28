namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppThingBitsTests
{
    [Fact]
    public void TryParseEntry_ValidatesBitRange()
    {
        Assert.False(VmDehackedDehSuppThingBits.TryParseEntry(40, 0, "X", out _, out var error));
        Assert.Contains("Invalid bit", error);
    }

    [Fact]
    public void TryParseEntry_TruncatesLongName()
    {
        Assert.True(VmDehackedDehSuppThingBits.TryParseEntry(
            1, 1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", out var entry, out _));
        Assert.Equal(VmDehackedDehSuppThingBits.MaxNameLength, entry.Name.Length);
    }

    [Fact]
    public void TryParseThingBitsSection_ParsesRows()
    {
        Assert.True(VmDehackedDehSuppThingBits.TryParseThingBitsSection(
            [(4, 1, "FRIEND"), (0, 0, "SOLID")],
            out var list,
            out _));
        Assert.Equal(2, list.Count);
    }
}
