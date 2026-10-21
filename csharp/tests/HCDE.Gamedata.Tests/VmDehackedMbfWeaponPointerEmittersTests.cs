namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfWeaponPointerEmittersTests
{
    [Fact]
    public void WeaponBulletAttackArgs_AppliesNativeDefaults() =>
        Assert.Equal((0f, 5, 3), VmDehackedMbfWeaponPointerEmitters.WeaponBulletAttackArgs(0, 0, 0));

    [Fact]
    public void WeaponMeleeAttackArgs_AppliesNativeDefaults()
    {
        var args = VmDehackedMbfWeaponPointerEmitters.WeaponMeleeAttackArgs(0, 0, 0, 0, 0);
        Assert.Equal(2, args.damage);
        Assert.Equal(1f, args.factor);
    }

    [Fact]
    public void SetDehParams_RejectsInvalidPointer() =>
        Assert.False(VmDehackedMbfWeaponPointerEmitters.SetDehParamsNative(-1, 10, 10, out _));
}
