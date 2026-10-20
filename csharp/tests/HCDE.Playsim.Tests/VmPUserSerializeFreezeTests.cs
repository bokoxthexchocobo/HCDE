namespace HCDE.Playsim.Tests;

public class VmPUserSerializeFreezeTests
{
    [Fact]
    public void Serialize_RollbackUsesAttackDown() =>
        Assert.True(VmPUserSerializeFreeze.SerializeRollbackBranch(true));

    [Fact]
    public void Serialize_WriteClearsButtons() =>
        Assert.True(VmPUserSerializeFreeze.SerializeWriteClearsUseButtons(true));

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
}
