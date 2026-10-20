namespace HCDE.Playsim.Tests;

public class VmPMobjPostBeginLifecycleTests
{
    [Fact]
    public void RecreateLights_WhenStateHasLight() =>
        Assert.True(VmPMobjPostBeginLifecycle.ShouldRecreateLights(false, true));

    [Fact]
    public void IsFast_AlwaysFastFlag() =>
        Assert.True(VmPMobjPostBeginLifecycle.IsFastMonster(VmPMobjPostBeginLifecycle.Mf5Alwaysfast, false));

    [Fact]
    public void Activate_WakesDormantMonster() =>
        Assert.Equal(VmPMobjPostBeginLifecycle.ActivateFlowResult.SetTics,
            VmPMobjPostBeginLifecycle.EvaluateActivate(Sample(true, false, false)));

    [Fact]
    public void Deactivate_SetsDormantPath() =>
        Assert.Equal(VmPMobjPostBeginLifecycle.ActivateFlowResult.SetTics,
            VmPMobjPostBeginLifecycle.EvaluateDeactivate(Sample(false, false, false)));

    [Fact]
    public void OnDestroy_PremorphWhenAlternative() =>
        Assert.True(VmPMobjPostBeginLifecycle.ShouldDestroyPremorph(
            new VmPMobjPostBeginLifecycle.OnDestroyPremorphContext(true, 0, false)));

    private static VmPMobjPostBeginLifecycle.ActivateFlowContext Sample(
        bool dormant, bool hasActive, bool ice) =>
        new(VmPMobjPostBeginLifecycle.Mf3Ismonster, ice ? 0 : 1,
            ice ? VmPMobjPostBeginLifecycle.MfIcecorpse : 0,
            dormant ? VmPMobjPostBeginLifecycle.Mf2Dormant : 0, hasActive);
}
