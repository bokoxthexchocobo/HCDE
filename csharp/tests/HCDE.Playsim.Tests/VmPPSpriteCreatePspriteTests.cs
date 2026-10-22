namespace HCDE.Playsim.Tests;

public class VmPPSpriteCreatePspriteTests
{
    [Fact]
    public void CreatePsprite_AppliesWeaponScaleWhenCallerIsWeapon()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel();
        var result = VmPPSpriteCreatePsprite.CreatePsprite(
            new object(),
            "weapon",
            1,
            (_, _, _) => pspr,
            w => w as string == "weapon",
            (_, name) => name == "WeaponScaleX" ? 2.0 : 0.5);
        Assert.Same(pspr, result);
        Assert.Equal(2.0, pspr.BaseScaleX);
        Assert.Equal(0.5, pspr.BaseScaleY);
    }
}
