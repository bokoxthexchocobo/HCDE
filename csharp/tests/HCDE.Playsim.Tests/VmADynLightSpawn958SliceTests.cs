namespace HCDE.Playsim.Tests;

public class VmADynLightSpawn958SliceTests
{
    [Fact]
    public void ShouldPopLightFromFreeList_WhenCountPositive()
    {
        Assert.True(VmADynLightSpawn958Slice.ShouldPopLightFromFreeList(1));
        Assert.False(VmADynLightSpawn958Slice.ShouldAllocateLightFromArena(1));
    }

    [Fact]
    public void ResolveUdmfSoftShadowRadius_OnlyWhenNonNegative()
    {
        Assert.Equal(2f, VmADynLightSpawn958Slice.ResolveUdmfSoftShadowRadius(2f, -1f));
        Assert.Equal(-1f, VmADynLightSpawn958Slice.ResolveUdmfSoftShadowRadius(-1f, -1f));
    }

    [Fact]
    public void ClearRecreateLightsFlag_ClearsMf8Bit()
    {
        var cleared = VmADynLightSpawn958Slice.ClearRecreateLightsFlag(
            VmADynLightAttach955Slice.Mf8RecreateLights);
        Assert.Equal(0, cleared);
    }

    [Fact]
    public void NormalizeSpawnAngleDegrees_WrapsNegative()
    {
        Assert.Equal(270, VmADynLightSpawn958Slice.NormalizeSpawnAngleDegrees(-90));
    }

    [Fact]
    public void InitialOffMapPosition_UsesInvalidX()
    {
        var (x, _, _) = VmADynLightSpawn958Slice.InitialOffMapPosition();
        Assert.Equal(VmADynLightSpawn958Slice.InvalidSpawnLightX, x);
    }

    [Fact]
    public void AbortActorLightAttach_WhenEuthanizing()
    {
        Assert.True(VmADynLightSpawn958Slice.AbortActorLightAttach(
            VmADynLightAttach955Slice.OfEuthanizeMe));
    }

    [Fact]
    public void ActivateAllAttachedLights_SkipsNullEntries()
    {
        var count = 0;
        VmADynLightSpawn958Slice.ActivateAllAttachedLights(
            [new object(), null],
            _ => count++);
        Assert.Equal(1, count);
    }

    [Fact]
    public void ArenaAllocationSize_ScalesByPoolCapacity()
    {
        Assert.Equal(400, VmADynLightSpawn958Slice.ArenaAllocationSize(2, 200));
    }

    [Fact]
    public void LinkNewLightHasSuccessor_WhenNextPresent()
    {
        Assert.True(VmADynLightSpawn958Slice.LinkNewLightHasSuccessor(new object()));
    }

    [Fact]
    public void ResolveUdmfLinearity_OnlyWhenNonNegative()
    {
        Assert.Equal(0.5f, VmADynLightSpawn958Slice.ResolveUdmfLinearity(0.5f, -1f));
    }

    [Fact]
    public void LightTypeFromSpawnInt_TruncatesToByte()
    {
        Assert.Equal((byte)3, VmADynLightSpawn958Slice.LightTypeFromSpawnInt(3));
    }

    [Fact]
    public void ShouldLinkPreviousLight_WhenNextPresent()
    {
        Assert.True(VmADynLightSpawn958Slice.ShouldLinkPreviousLight(new object()));
    }
}
