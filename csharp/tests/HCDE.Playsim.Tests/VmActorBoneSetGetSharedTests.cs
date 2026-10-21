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
        Assert.False(VmActorBoneSetGetShared.TryResolveNamedBoneIndex(-1, 4, out _));

    [Fact]
    public void NeedsBoneOverrideModelSlotGrow_WhenSizeTooSmall() =>
        Assert.True(VmActorBoneSetGetShared.NeedsBoneOverrideModelSlotGrow(1, 2));

    [Fact]
    public void SetGetBoneSharedIndexGate_NamedBoneMissing()
    {
        Assert.False(VmActorBoneSetGetShared.SetGetBoneSharedIndexGate(
            true, true, true, false, true, -1, 0, 4, out var missing));
        Assert.True(missing);
    }

    [Fact]
    public void TryPrepareSetBoneOffset_InvalidMode_Fails()
    {
        Assert.False(VmActorBoneSetGetShared.TryPrepareSetBoneOffset(-2, 9, 1, 4, out var dur));
        Assert.Equal(0, dur);
    }

    [Fact]
    public void TryPrepareSetBoneOffset_ValidIndex_Ok() =>
        Assert.True(VmActorBoneSetGetShared.TryPrepareSetBoneOffset(1, 1, 1, 4, out _));

    [Fact]
    public void BoneRotationShouldCalcBones_WhenModelPresent() =>
        Assert.True(VmActorBoneSetGetShared.BoneRotationShouldCalcBones(true));

    [Fact]
    public void DefaultActorModelIndex_IsZero() =>
        Assert.Equal(0, VmActorBoneSetGetShared.DefaultActorModelIndex);
}
