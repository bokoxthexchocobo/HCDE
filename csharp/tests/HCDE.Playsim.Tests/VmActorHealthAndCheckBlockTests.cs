namespace HCDE.Playsim.Tests;

public class VmActorHealthAndCheckBlockTests
{
    [Fact]
    public void ResolveHealthAssignment_ClampsNonPositiveToOne() =>
        Assert.Equal(1, VmActorHealthAndCheckBlock.ResolveHealthAssignment(0, isPlayer: true));

    [Fact]
    public void ShouldSkipHealthPointer_WhenNull() =>
        Assert.True(VmActorHealthAndCheckBlock.ShouldSkipHealthPointer(false));

    [Fact]
    public void ShouldResetPlayerHealth_WhenAlive() =>
        Assert.True(VmActorHealthAndCheckBlock.ShouldResetPlayerHealth(true, 50));

    [Fact]
    public void ClampChaseThreshold_NegativeBecomesZero() =>
        Assert.Equal(0, VmActorHealthAndCheckBlock.ClampChaseThreshold(-5));

    [Fact]
    public void MergeCheckBlockAngle_AddsYawUnlessAbsolute() =>
        Assert.Equal(90, VmActorHealthAndCheckBlock.MergeCheckBlockAngle(45, 45, default));

    [Fact]
    public void CheckBlockReturnValue_DropoffBranch() =>
        Assert.True(VmActorHealthAndCheckBlock.CheckBlockReturnValue(
            VmActorHealthAndCheckBlock.CbfFlags.Dropoff,
            blockingActorPresent: false,
            blockingLinePresent: false,
            dropoffCheckFailed: true));

    [Fact]
    public void PlanCheckBlockOffset_AbsolutePos() =>
        Assert.Equal((1, 2, 3), VmActorHealthAndCheckBlock.PlanCheckBlockOffset(
            1, 2, 3, 0, VmActorHealthAndCheckBlock.CbfFlags.AbsolutePos, (_, _, _) => (0, 0, 0)));
}
