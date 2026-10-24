namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchSpriteTableTests
{
    [Fact]
    public void IsValidSpriteName_RequiresFourChars()
    {
        Assert.True(VmDehackedPatchSpriteTable.IsValidSpriteName("PLAY"));
        Assert.False(VmDehackedPatchSpriteTable.IsValidSpriteName("P"));
    }

    [Fact]
    public void EnsureTableSize_GrowsForIndex()
    {
        var table = new List<string> { "A" };
        VmDehackedPatchSpriteTable.EnsureTableSize(table, 2);
        Assert.Equal(3, table.Count);
    }
}
