namespace HCDE.Playsim.Tests;

public class VmActorBonePoseGettersTests
{
    [Fact]
    public void GetAnimStartFrameNative_ReturnsFirstFrame() =>
        Assert.Equal(7, VmActorBonePoseGetters.GetAnimStartFrameNative(7));

    [Fact]
    public void GetBoneFramePoseNative_CapsNumret() =>
        Assert.Equal(3, VmActorBonePoseGetters.GetBoneFramePoseNative(
            true, 0, 0, 10, 5, false, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0));

    [Fact]
    public void GetBoneBasePositionNative_ReturnsPosition() =>
        Assert.Equal((1, 2, 3), VmActorBonePoseGetters.GetBoneBasePositionNative(true, 1, 2, 3));

    [Fact]
    public void GetBoneTrsNative_UsesActorTrs() =>
        Assert.Equal(2, VmActorBonePoseGetters.GetBoneTrsNative(
            true, false, 0, 2, true, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0));
}
