namespace HCDE.Playsim.Tests;

public class VmActionTargetInLosTests
{
    [Fact]
    public void FailWhenNoTarget() =>
        Assert.True(VmActionTargetInLos.FailWhenNoTarget(null));

    [Fact]
    public void FailWhenDeadNoJump() =>
        Assert.True(VmActionTargetInLos.FailWhenDeadNoJump(VmActionTargetInLos.JlosfDeadnojump, 0));

    [Fact]
    public void BeyondDistMax_WhenExceeded() =>
        Assert.True(VmActionTargetInLos.BeyondDistMax(200, 100));

    [Fact]
    public void CloseNoJump_FlagSet() =>
        Assert.True(VmActionTargetInLos.CloseNoJump(VmActionTargetInLos.JlosfClosenojump));

    [Fact]
    public void UseTargetLosViewport_SwapsRoles() =>
        Assert.True(VmActionTargetInLos.UseTargetLosViewport(VmActionTargetInLos.JlosfTargetlos));

    [Fact]
    public void ClampFovDegrees_CapsAt360() =>
        Assert.Equal(360, VmActionTargetInLos.ClampFovDegrees(400));

    [Fact]
    public void OutsideHalfFov_RejectsWideAngle() =>
        Assert.True(VmActionTargetInLos.OutsideHalfFov(50, 60));
}
