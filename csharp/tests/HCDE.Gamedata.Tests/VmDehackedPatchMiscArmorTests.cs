namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMiscArmorTests
{
    [Fact]
    public void NormalizeInfighting_MapsDd()
    {
        Assert.Equal(1, VmDehackedPatchMiscArmor.NormalizeInfighting(0xDD));
    }
}
