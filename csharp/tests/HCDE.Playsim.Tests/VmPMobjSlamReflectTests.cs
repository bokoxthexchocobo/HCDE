namespace HCDE.Playsim.Tests;

public class VmPMobjSlamReflectTests
{
    [Fact]
    public void OnlySlamSolid_PassesNonSolid() =>
        Assert.True(VmPMobjSlamReflect.OnlySlamSolidBlocks(VmPMobjSlamReflect.Mf8Onlyslamsolid, 0));

    [Fact]
    public void EvaluateSlamPass_WhenOnlySolid() =>
        Assert.True(VmPMobjSlamReflect.EvaluateSlamPass(
            new VmPMobjSlamReflect.SlamContext(1, 0, 0, 1, false, true, true, false)));

    [Fact]
    public void ShieldReflect_BlocksWideAngle() =>
        Assert.False(VmPMobjSlamReflect.ShieldReflectAngleGate(0, 90));

    [Fact]
    public void IsOkayToAttack_RejectsSelf() =>
        Assert.False(VmPMobjSlamReflect.IsOkayToAttackGate(
            new VmPMobjSlamReflect.AttackEligibilityContext(
                true, true, true, false, 0, false, false, false, false, false, false, true, false, 0)));

    [Fact]
    public void ScreenSeeker_AllowsNarrowFov() =>
        Assert.True(VmPMobjSlamReflect.IsOkayToAttackGate(
            new VmPMobjSlamReflect.AttackEligibilityContext(
                false, true, true, false, 0, false, false, false, false, false, false, true, true, 10)));
}
