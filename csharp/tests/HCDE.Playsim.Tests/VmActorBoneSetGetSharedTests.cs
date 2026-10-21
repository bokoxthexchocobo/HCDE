namespace HCDE.Playsim.Tests;

public class VmActorBoneSetGetSharedTests
{
    [Fact]
    public void NormalizeInterpolationDuration_ClampsNegative() =>
        Assert.Equal(0, VmActorBoneSetGetShared.NormalizeInterpolationDuration(-1));

    [Fact]
    public void IsValidSetBoneMode_RejectsAboveTwo() =>
        Assert.False(VmActorBoneSetGetShared.IsValidSetBoneMode(3));

    [Fact]
    public void TryValidateBoneIndex_RejectsOutOfRange() =>
        Assert.False(VmActorBoneSetGetShared.TryValidateBoneIndex(5, 3, out _));

    [Fact]
    public void TryResolveNamedBoneIndex_FailsWhenMissing() =>
        Assert.False(VmActorBoneSetGetShared.TryResolveNamedBoneIndex(-1, 4, out var msg) && msg is null);

    [Fact]
    public void PlanBoneOverrideResize_GrowsModelSlotWhenNeeded()
    {
        var plan = VmActorBoneSetGetShared.PlanBoneOverrideResize(1, 2, 8);
        Assert.True(plan.NeedsModelSlotGrow);
        Assert.Equal(2, plan.ModelIndex);
    }

    [Fact]
    public void PlanSetGetBoneSharedIndex_NamedBoneMissing_ReturnsMissing()
    {
        var res = VmActorBoneSetGetShared.PlanSetGetBoneSharedIndex(
            true, true, true, false, true, -1, 0, 4);
        Assert.Equal(VmActorBoneSetGetShared.FModelResolution.MissingNamedBone, res);
    }

    [Fact]
    public void PlanSetBoneOffsetShared_InvalidMode_Aborts()
    {
        var (res, dur) = VmActorBoneSetGetShared.PlanSetBoneOffsetShared(
            true, -2, 9, true, true, 1, 4);
        Assert.Equal(VmActorBoneSetGetShared.FModelResolution.Abort, res);
        Assert.Equal(0, dur);
    }

    [Fact]
    public void PlanGetBoneOffsetShared_ValidIndex_Ok()
    {
        var res = VmActorBoneSetGetShared.PlanGetBoneOffsetShared(
            true, true, true, false, 0, 1, 4);
        Assert.Equal(VmActorBoneSetGetShared.FModelResolution.Ok, res);
    }

    [Fact]
    public void BoneRotationNativeShouldCalcBones_WhenOk() =>
        Assert.True(VmActorBoneSetGetShared.BoneRotationNativeShouldCalcBones(
            VmActorBoneSetGetShared.FModelResolution.Ok));

    [Fact]
    public void PackBoneRotation_ForwardsComponents()
    {
        var (x, y, z, w) = VmActorBoneSetGetShared.PackBoneRotation(1, 2, 3, 4);
        Assert.Equal((1, 2, 3, 4), (x, y, z, w));
    }

    [Fact]
    public void PackBoneTranslation_ForwardsComponents() =>
        Assert.Equal((1, 2, 3), VmActorBoneSetGetShared.PackBoneTranslation(1, 2, 3));

    [Fact]
    public void DefaultModelIndexForActorBone_IsZero() =>
        Assert.Equal(0, VmActorBoneSetGetShared.DefaultModelIndexForActorBone());

    [Fact]
    public void DecoupledAnimationsError_SetOffsetMessage() =>
        Assert.Contains("set bone offset", VmActorBoneSetGetShared.DecoupledAnimationsError(
            VmActorBoneSetGetShared.BoneAccessKind.SetBoneOffset, true));
}
