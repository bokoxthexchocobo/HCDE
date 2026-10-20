namespace HCDE.Playsim.Tests;

public class VmPMobjTagDamageRestoreTests
{
    [Fact]
    public void Tag_StringTableWhenDollar() =>
        Assert.True(VmPMobjTagDamageRestore.TagUsesStringTable(true, true));

    [Fact]
    public void ClearCounters_KillWhenAlive() =>
        Assert.True(VmPMobjTagDamageRestore.ClearKillCounter(true, 50));

    [Fact]
    public void DamageFactor_ScalesPositive() =>
        Assert.Equal(20, VmPMobjTagDamageRestore.ApplyPositiveDamageFactor(10, 2.0));

    [Fact]
    public void Translation_EmptyResets() =>
        Assert.True(VmPMobjTagDamageRestore.EmptyTranslationResets(true));

    [Fact]
    public void Restore_SpawnFloatNeedsSpace() =>
        Assert.True(VmPMobjTagDamageRestore.SpawnFloatSpace(200, 0, 10));

    [Fact]
    public void Restore_ClampFloor() =>
        Assert.True(VmPMobjTagDamageRestore.ClampBelowFloor(5, 10));
}
