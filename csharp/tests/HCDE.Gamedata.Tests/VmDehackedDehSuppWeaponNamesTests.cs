namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppWeaponNamesTests
{
    [Fact]
    public void ParseWeaponNameList_ClearsAndAppendsTypes()
    {
        var list = VmDehackedDehSuppWeaponNames.ParseWeaponNameList(["Fist", ",", "Pistol", "Shotgun"]);
        Assert.Equal(3, list.Count);
        Assert.Equal("Fist", list[0]);
        Assert.Equal("Shotgun", list[2]);
    }

    [Fact]
    public void TryParseWeaponEntry_RejectsEmpty()
    {
        Assert.False(VmDehackedDehSuppWeaponNames.TryParseWeaponEntry("", out _));
    }
}
