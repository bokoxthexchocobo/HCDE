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
    public void MonsterBulletAttackArgs_AppliesNativeDefaults() =>
        Assert.Equal((0f, 0f, 1, 5, 3), VmDehackedMbfWeaponPointerEmitters.MonsterBulletAttackArgs(0, 0, 0, 0, 0));

    [Fact]
    public void RadiusDamageArgs_PassesInts() =>
        Assert.Equal((7, 12), VmDehackedMbfWeaponPointerEmitters.RadiusDamageArgs(7, 12));

    [Fact]
    public void SeekTracerArgs_PassesFloats() =>
        Assert.Equal((1.5f, 2f), VmDehackedMbfWeaponPointerEmitters.SeekTracerArgs(1.5f, 2f));

    [Fact]
    public void CreateSpawnObjectNative_ValidatesArgCount() =>
        Assert.Throws<ArgumentException>(() => VmDehackedMbfWeaponPointerEmitters.CreateSpawnObjectNative(7));

    [Fact]
    public void SetDehParams_RejectsInvalidPointer() =>
        Assert.False(VmDehackedMbfWeaponPointerEmitters.SetDehParamsNative(-1, 10, 10, out _));

    [Fact]
    public void SetDehParams_ReturnsStateWhenCodePointerSix()
    {
        Assert.True(VmDehackedMbfWeaponPointerEmitters.SetDehParamsNative(6, 10, 10, out var returnsState));
        Assert.True(returnsState);
    }
}
