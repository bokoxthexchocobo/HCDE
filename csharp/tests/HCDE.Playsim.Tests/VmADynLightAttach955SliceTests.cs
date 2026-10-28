namespace HCDE.Playsim.Tests;

public class VmADynLightAttach955SliceTests
{
    [Fact]
    public void LightAssociationMatches_AnyFrameWildcard()
    {
        Assert.True(VmADynLightAttach955Slice.LightAssociationMatches(
            1, 2, 1, VmADynLightAttach955Slice.AnyFrameAssociation));
    }

    [Fact]
    public void ShouldFallbackToStateLights_WhenNoAttachmentsYet()
    {
        Assert.True(VmADynLightAttach955Slice.ShouldFallbackToStateLights(0, 3));
        Assert.False(VmADynLightAttach955Slice.ShouldFallbackToStateLights(1, 3));
    }

    [Fact]
    public void FindUserLightIndex_NotFoundWithoutCreate()
    {
        var idx = VmADynLightAttach955Slice.FindUserLightIndex(
            ["a"], "b", false, out var created);
        Assert.Equal(VmADynLightAttach955Slice.UserLightNotFound, idx);
        Assert.False(created);
    }

    [Fact]
    public void AttachLightTypeParameter_ScalesPulseWithTicrate()
    {
        Assert.Equal(35.0, VmADynLightAttach955Slice.AttachLightTypeParameter(
            VmADynLightTick.PulseLight, 1.0));
    }

    [Fact]
    public void SpotPitchInRange_AcceptsPlusMinusNinety()
    {
        Assert.True(VmADynLightAttach955Slice.SpotPitchInRange(90));
        Assert.False(VmADynLightAttach955Slice.SpotPitchInRange(91));
    }

    [Fact]
    public void ExtractColorComponents_SplitsRgb()
    {
        Assert.Equal(0x12, VmADynLightAttach955Slice.ExtractColorRed(0x12_34_56));
        Assert.Equal(0x34, VmADynLightAttach955Slice.ExtractColorGreen(0x12_34_56));
        Assert.Equal(0x56, VmADynLightAttach955Slice.ExtractColorBlue(0x12_34_56));
    }

    [Fact]
    public void AttachLightOffsetComponents_SwapsYIntoMiddleSlot()
    {
        var ofs = VmADynLightAttach955Slice.AttachLightOffsetComponents(1, 2, 3);
        Assert.Equal([1f, 3f, 2f], ofs);
    }

    [Fact]
    public void AbortLightAttachWhenEuthanizing_WhenFlagSet()
    {
        Assert.True(VmADynLightAttach955Slice.AbortLightAttachWhenEuthanizing(
            VmADynLightAttach955Slice.OfEuthanizeMe));
    }

    [Fact]
    public void RemoveLightIndexValid_BeforeCount()
    {
        Assert.True(VmADynLightAttach955Slice.RemoveLightIndexValid(1, 2));
        Assert.False(VmADynLightAttach955Slice.RemoveLightIndexValid(2, 2));
    }

    [Fact]
    public void UserLightSlotAfterCreate_UsesPreviousCountWhenCreated()
    {
        Assert.Equal(2, VmADynLightAttach955Slice.UserLightSlotAfterCreate(2, true, 2));
        Assert.Equal(1, VmADynLightAttach955Slice.UserLightSlotAfterCreate(1, false, 2));
    }
}
