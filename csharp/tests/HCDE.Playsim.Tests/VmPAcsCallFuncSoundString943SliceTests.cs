namespace HCDE.Playsim.Tests;

public class VmPAcsCallFuncSoundString943SliceTests
{
    [Fact]
    public void SoundVolumeUsesActivator_WhenTidZero() =>
        Assert.True(VmPAcsCallFuncSoundString943Slice.SoundVolumeUsesActivator(0));

    [Fact]
    public void StringCompareSameIndex_ReturnsZeroWithoutLookup() =>
        Assert.True(VmPAcsCallFuncSoundString943Slice.StringCompareSameIndex(3, 3));

    [Fact]
    public void TrimSubstringLength_ClampsToOldLen() =>
        Assert.Equal(4, VmPAcsCallFuncSoundString943Slice.TrimSubstringLength(4, 9));

    [Fact]
    public void StrMidPosClamped_ReturnsNegativeWhenPastEnd() =>
        Assert.Equal(-1, VmPAcsCallFuncSoundString943Slice.StrMidPosClamped(5, 3));

    [Fact]
    public void SpawnDecalDistance_DefaultsToSixtyFour() =>
        Assert.Equal(64, VmPAcsCallFuncSoundString943Slice.SpawnDecalDistance(2, 0, 0, 64));

    [Fact]
    public void GetWeaponReturnsNone_WhenActivatorMissing() =>
        Assert.True(VmPAcsCallFuncSoundString943Slice.GetWeaponReturnsNone(null, new object()));

    [Fact]
    public void NullLookupToEmpty_UsesEmptyString() =>
        Assert.Equal(string.Empty, VmPAcsCallFuncSoundString943Slice.NullLookupToEmpty(null));
}
