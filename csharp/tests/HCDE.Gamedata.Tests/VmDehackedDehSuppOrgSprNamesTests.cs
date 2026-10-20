namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppOrgSprNamesTests
{
    [Fact]
    public void TryParseSpriteName_RequiresFourChars()
    {
        Assert.True(VmDehackedDehSuppOrgSprNames.TryParseSpriteName("PLAY", out var name));
        Assert.Equal("PLAY", name);
        Assert.False(VmDehackedDehSuppOrgSprNames.TryParseSpriteName("P", out _));
    }
}
