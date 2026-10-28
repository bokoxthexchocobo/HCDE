namespace HCDE.Playsim.Tests;

public class VmPLnspecThingHate943SliceTests
{
    [Fact]
    public void HateeCandidateValid_RequiresShootableAliveAwake() =>
        Assert.True(VmPLnspecThingHate943Slice.HateeCandidateValid(VmPLnspecThingHate943Slice.MfShootable, 10, 0));

    [Fact]
    public void NothingToHateWhenTidNonZero_DetectsMissingHatee() =>
        Assert.True(VmPLnspecThingHate943Slice.NothingToHateWhenTidNonZero(null, 5));

    [Fact]
    public void RejectPlayerAsHater_BlocksActivatorPlayer() =>
        Assert.True(VmPLnspecThingHate943Slice.RejectPlayerAsHater(true, 0));

    [Fact]
    public void BuildHateAttitude_SetsNoSightForMode2() =>
        Assert.NotEqual(0, VmPLnspecThingHate943Slice.BuildHateAttitude(0, 0, 2).Flags3 & VmPLnspecThingHate943Slice.Mf3NoSightCheck);

    [Fact]
    public void CanChaseAfterHate_RejectsSelf() =>
        Assert.False(VmPLnspecThingHate943Slice.CanChaseAfterHate(new object(), new object(), null, null, 0));

    [Fact]
    public void HateeScanContinue_StopsOnDormant() =>
        Assert.True(VmPLnspecThingHate943Slice.HateeScanContinue(new object(), null, VmPLnspecThingHate943Slice.MfShootable, 5, VmPLnspecThingHate943Slice.Mf2Dormant));

    [Fact]
    public void ShouldWakeSeeState_RequiresPositiveHealth() =>
        Assert.True(VmPLnspecThingHate943Slice.ShouldWakeSeeState(0, 1));
}
