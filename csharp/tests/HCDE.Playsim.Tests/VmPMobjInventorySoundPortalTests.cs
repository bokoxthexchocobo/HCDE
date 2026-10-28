namespace HCDE.Playsim.Tests;

public class VmPMobjInventorySoundPortalTests
{
    [Fact]
    public void PlayActiveSound_RequiresFreeVoice() =>
        Assert.True(VmPMobjInventorySoundPortal.EvaluatePlayActiveSound(
            new VmPMobjInventorySoundPortal.PlayActiveSoundContext(true, false, 0)));

    [Fact]
    public void PlayActiveSound_BlocksWhenBusy() =>
        Assert.False(VmPMobjInventorySoundPortal.EvaluatePlayActiveSound(
            new VmPMobjInventorySoundPortal.PlayActiveSoundContext(true, true, 0)));

    [Fact]
    public void PlayPushSound_WhenBodyFree() =>
        Assert.True(VmPMobjInventorySoundPortal.EvaluatePlayPushSound(
            new VmPMobjInventorySoundPortal.PlayPushSoundContext(false)));

    [Fact]
    public void ActiveAttenuation_FullVolumeFlag() =>
        Assert.Equal(0, VmPMobjInventorySoundPortal.ActiveAttenuation(
            VmPMobjInventorySoundPortal.Mf3Fullvolactive));

    [Fact]
    public void GetPortalTransition_MovesThroughCeiling() =>
        Assert.True(VmPMobjInventorySoundPortal.EvaluateGetPortalTransition(
            new VmPMobjInventorySoundPortal.PortalTransitionProbeContext(
                32, 0, false, 16, true, 0, false)).Moved);

    [Fact]
    public void CheckPortal_CeilingStepWhenAtPlane() =>
        Assert.True(VmPMobjInventorySoundPortal.EvaluateCeilingPortalStep(
            new VmPMobjInventorySoundPortal.CheckPortalTransitionContext(
                64, 64, 0, 0, false, true, true, false)));

    [Fact]
    public void CheckPortal_FloorStepWhenBelowPlane() =>
        Assert.True(VmPMobjInventorySoundPortal.EvaluateFloorPortalStep(
            new VmPMobjInventorySoundPortal.CheckPortalTransitionContext(
                8, 64, 16, 8, true, false, true, false)));

    [Fact]
    public void ModelTimer_ClientUsesLocal() =>
        Assert.Equal(99, VmPMobjInventorySoundPortal.EvaluateModelTimer(true, 99, 1));

    [Fact]
    public void ReverseInventoryOrder_WalksBackwards()
    {
        var order = VmPMobjInventorySoundPortal.ReverseInventoryOrder(3).ToArray();
        Assert.Equal(new[] { 2, 1, 0 }, order);
    }
}
