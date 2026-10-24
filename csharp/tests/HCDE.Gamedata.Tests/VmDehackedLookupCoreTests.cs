namespace HCDE.Gamedata.Tests;

public class VmDehackedLookupCoreTests
{
    [Fact]
    public void DsSoundLumpName_PrefixesDs()
    {
        Assert.Equal("dsplasma", VmDehackedLookupCore.DsSoundLumpName("plasma"));
    }

    [Fact]
    public void ResolveInfoNameIndex_RespectsBounds()
    {
        Assert.Equal(1, VmDehackedLookupCore.ResolveInfoNameIndex(1, 5, false));
    }

    [Fact]
    public void IsValidSoundIndex_RespectsBounds()
    {
        Assert.True(VmDehackedLookupCore.IsValidSoundIndex(0, 3));
        Assert.False(VmDehackedLookupCore.IsValidSoundIndex(5, 3));
    }
}
