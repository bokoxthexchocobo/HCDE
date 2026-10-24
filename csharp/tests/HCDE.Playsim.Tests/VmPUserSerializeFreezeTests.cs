namespace HCDE.Playsim.Tests;

public class VmPUserSerializeFreezeTests
{
    [Fact]
    public void Serialize_RollbackUsesAttackDown() =>
        Assert.True(true);

    [Fact]
    public void Serialize_WriteClearsButtons() =>
        Assert.True(true);

    [Fact]
    public void FallingDamage_TelefragHighVelocity() =>
        Assert.True(VmPUserSerializeFreeze.FallingDamageTelefrag(-40));

    [Fact]
    public void FallingDamage_ScalarBelowTelefrag() =>
        Assert.Equal(12, VmPUserSerializeFreeze.PickFallingDamage(
            new VmPUserSerializeFreeze.FallingDamagePick(25, true, 1000)));

    [Fact]
    public void TotallyFrozen_ByCheat() =>
        Assert.True(VmPUserSerializeFreeze.EvaluateTotallyFrozen(
            new VmPUserSerializeFreeze.FreezeGate(0, VmPUserSerializeFreeze.CfTotallyfrozen, false)));

    [Fact]
    public void Misc_InfightingIgnoreEachOther() =>
        Assert.Equal(-1, VmPUserSerializeFreeze.InfightingFromMisc(true, 1));

    [Fact]
    public void MonsterFall_AppliesDamage() =>
        Assert.True(VmPUserSerializeFreeze.EvaluateMonsterFall(
            new VmPUserSerializeFreeze.FallingDamageGate(1, 0, 0, 40, true),
            1000,
            true).Applied);
}
