namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchSpriteTableTests
{
    [Fact]
    public void IsValidSpriteName_RequiresFourChars()
    {
        Assert.True(VmDehackedPatchSpriteTable.IsValidSpriteName("PLAY"));
        Assert.False(VmDehackedPatchSpriteTable.IsValidSpriteName("P"));
    }
}
