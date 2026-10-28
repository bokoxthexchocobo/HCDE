namespace HCDE.Playsim.Tests;

public class VmPAcsCallFuncDropGameplay944SliceTests
{
    [Fact]
    public void DropItemAmount_DefaultsToNegativeOne() =>
        Assert.Equal(-1, VmPAcsCallFuncDropGameplay944Slice.DropItemAmountOrDefault(2, 0));

    [Fact]
    public void DropItemChance_DefaultsToTwoFiftySix() =>
        Assert.Equal(256, VmPAcsCallFuncDropGameplay944Slice.DropItemChanceOrDefault(3, 99));

    [Fact]
    public void QuakeExDamageMultiplier_DefaultsToOne() =>
        Assert.Equal(1.0, VmPAcsCallFuncDropGameplay944Slice.ResolveQuakeExDamageMultiplier(8, 0));

    [Fact]
    public void QuakeExHighFrequency_DefaultsToHalf() =>
        Assert.Equal(0.5, VmPAcsCallFuncDropGameplay944Slice.ResolveQuakeExHighFrequency(17, 0));

    [Fact]
    public void SetLineActivationRepeat_DefaultsToNegativeOne() =>
        Assert.Equal(-1, VmPAcsCallFuncDropGameplay944Slice.SetLineActivationRepeatOrDefault(2, 1));

    [Fact]
    public void GetLineActivation_ReturnsZeroWhenLineMissing() =>
        Assert.Equal(0, VmPAcsCallFuncDropGameplay944Slice.GetLineActivationWhenFound(-1, 7));

    [Fact]
    public void ChangeActorAngleInterpolate_FalseWhenArgMissing() =>
        Assert.False(VmPAcsCallFuncDropGameplay944Slice.ChangeActorAngleInterpolate(2, 1));
}
