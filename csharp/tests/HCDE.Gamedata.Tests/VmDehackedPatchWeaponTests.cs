namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchWeaponTests
{
    [Fact]
    public void ClampAmmoTypeIndex_FallsBackToFive()
    {
        Assert.Equal(5, VmDehackedPatchWeapon.ClampAmmoTypeIndex(99, 4));
    }

    [Fact]
    public void TryMapWeaponFrameLabel_SwapsSelectDeselect()
    {
        var labels = new Dictionary<string, int>();
        Assert.True(VmDehackedPatchWeapon.TryMapWeaponFrameLabel("Select frame", 3, labels));
        Assert.Equal(3, labels["Deselect"]);
    }
}
