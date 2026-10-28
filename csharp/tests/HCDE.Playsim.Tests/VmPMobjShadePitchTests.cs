namespace HCDE.Playsim.Tests;

public class VmPMobjShadePitchTests
{
    [Fact]
    public void ClampPitch_NonPlayerUses89() =>
        Assert.Equal(0, VmPMobjShadePitch.EvaluateClampPitch(
            new VmPMobjShadePitch.ClampPitchContext(false, 0, 0, 0)));

    [Fact]
    public void SetPitch_DetectsChange() =>
        Assert.True(VmPMobjShadePitch.EvaluateSetPitch(
            new VmPMobjShadePitch.SetPitchContext(true, VmPMobjShadePitch.SpfInterpolate, 0, 1)));

    [Fact]
    public void SetAngle_ScaledNoLerp() =>
        Assert.True(VmPMobjShadePitch.EvaluateSetAngle(
            new VmPMobjShadePitch.SetAngleContext(true, VmPMobjShadePitch.SpfScalednolerp, 0, 1)));

    [Fact]
    public void SetViewPitch_InterpAngles() =>
        Assert.True(VmPMobjShadePitch.EvaluateSetViewPitch(
            new VmPMobjShadePitch.SetViewPitchContext(true, VmPMobjShadePitch.SpfInterpolate, 0, 1)));

    [Fact]
    public void GetFov_LerpsWhenInterp() =>
        Assert.Equal(50, VmPMobjShadePitch.EvaluateGetFov(
            new VmPMobjShadePitch.GetFovContext(false, 0, 90, 60, 40, 0.5)));
}
