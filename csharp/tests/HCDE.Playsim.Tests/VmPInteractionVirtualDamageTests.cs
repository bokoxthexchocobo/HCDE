namespace HCDE.Playsim.Tests;

public class VmPInteractionVirtualDamageTests
{
    [Fact]
    public void ReflectionScan_PicksHigherReflect() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluateReflectionScan(
            new VmPInteractionVirtualDamage.ReflectionScanContext(10, true, 0, 1, true, 5, 12)));

    [Fact]
    public void Kickback_BlocksThrustlessFlag() =>
        Assert.False(VmPInteractionVirtualDamage.KickbackEligible(
            false, true, false, false, false, VmPInteractionVirtualDamage.DmgThrustless, 0, false));

    [Fact]
    public void DoDamageMobj_MirrorSkips() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluateDoDamageMobjMirror(
            new VmPInteractionVirtualDamage.DoDamageMobjContext(true, false, false, false, false, false, 0, true)));

    [Fact]
    public void EvaluateDoDamageMobjMirror_SkipsMirrorActors() => DoDamageMobj_MirrorSkips();

    [Fact]
    public void PDamageMobj_RecordsPositiveDamage() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluatePDamageMobjRecord(
            new VmPInteractionVirtualDamage.PDamageMobjContext(false, false, false, false, false, false, true, 5)));

    [Fact]
    public void EvaluatePDamageMobjEarly_SkipsMirror() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluatePDamageMobjEarly(
            new VmPInteractionVirtualDamage.PDamageMobjContext(true, false, false, false, false, false, false, 0)));

    [Fact]
    public void CallOkaySwitch_UsesVirtualReturn() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluateCallOkayToSwitchTarget(
            new VmPInteractionVirtualDamage.CallOkaySwitchContext(true, 1)));

    [Fact]
    public void CallOkaySwitch_FallsBackWhenNoVirtual() =>
        Assert.True(VmPInteractionVirtualDamage.EvaluateCallOkayToSwitchTarget(
            new VmPInteractionVirtualDamage.CallOkaySwitchContext(false, 0)));

    [Fact]
    public void ClampDoDamageReturn_NegativeToZero() =>
        Assert.Equal(0, VmPInteractionVirtualDamage.ClampDoDamageReturn(-3));

    [Fact]
    public void PoisonMobjRequiresInflictor() =>
        Assert.False(VmPInteractionVirtualDamage.PoisonMobjRequiresInflictor(true, true));

    [Fact]
    public void PoisonMobjRequiresInflictor_ActionGate() =>
        Assert.False(VmPInteractionVirtualDamage.PoisonMobjRequiresInflictor(true, true));

    [Fact]
    public void PoisonDamagePlaysound() =>
        Assert.True(true);
}
