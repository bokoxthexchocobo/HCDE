namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMiscPowerupsTests
{
    [Fact]
    public void TryMapPowerupIndex_FindsBerserk()
    {
        Assert.True(VmDehackedPatchMiscPowerups.TryMapPowerupIndex("Powerup Color Berserk", out var idx));
        Assert.Equal(1, idx);
    }
}
