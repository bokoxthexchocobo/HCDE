namespace HCDE.Client.Tests;

public class VmAmMapFakeFlat1134SliceTests
{
    [Fact]
    public void ViewerZAndArea_PlayerViewAndBelowOrAboveTheControl()
    {
        Assert.Equal(48, VmAmMapFakeFlat1134Slice.ViewerZ(true, 48, 10, 16));
        Assert.Equal(26, VmAmMapFakeFlat1134Slice.ViewerZ(false, 48, 10, 16));
        Assert.Equal(0, VmAmMapFakeFlat1134Slice.ViewerArea(false, 0, 0, 128, 0));
        Assert.Equal(-1, VmAmMapFakeFlat1134Slice.ViewerArea(true, 0, 0, 128, 0));
        Assert.Equal(1, VmAmMapFakeFlat1134Slice.ViewerArea(true, 200, 0, 128, 0));
        Assert.Equal(0, VmAmMapFakeFlat1134Slice.ViewerArea(true, 200, 0, 128, VmAmMapFakeFlat1134Slice.FakeFloorOnly));
    }

    [Fact]
    public void Apply_FakeFloorOnlyBelowCopiesLightAndOtherwiseKeepsTheSector()
    {
        var flags = VmAmMapFakeFlat1134Slice.ClipFakePlanes | VmAmMapFakeFlat1134Slice.FakeFloorOnly;
        var below = VmAmMapFakeFlat1134Slice.Apply(true, 0, 0, 128, 0, flags, false, false);
        Assert.True(below.UseDestination);
        Assert.True(below.CopiedColormap);
        Assert.True(below.CopiedLight);
        var inside = VmAmMapFakeFlat1134Slice.Apply(true, 64, 0, 128, 0, flags, false, false);
        Assert.False(inside.UseDestination);
    }

    [Fact]
    public void Apply_BelowRestoresTheTargetFloorAndAboveKeepsSkyCeiling()
    {
        var below = VmAmMapFakeFlat1134Slice.Apply(
            true, -1, 0, 128, 0, VmAmMapFakeFlat1134Slice.ClipFakePlanes, true, false);
        Assert.True(below.RestoredTargetFloor);
        Assert.Equal(VmAmMapFakeFlat1134Slice.TextureTargetFloor, below.FloorTextureSource);
        var sky = VmAmMapFakeFlat1134Slice.Apply(true, 200, 0, 128, 0, 0, false, true);
        Assert.True(sky.UsedCeilingAsFloor);
        Assert.Equal(VmAmMapFakeFlat1134Slice.TextureControlCeiling, sky.FloorTextureSource);
        var floor = VmAmMapFakeFlat1134Slice.Apply(true, 200, 0, 128, 0, VmAmMapFakeFlat1134Slice.NoFakeLight, false, false);
        Assert.Equal(VmAmMapFakeFlat1134Slice.TextureControlFloor, floor.FloorTextureSource);
        Assert.False(floor.CopiedLight);
    }

    [Fact]
    public void SkipSubsector_HidesPolyOrgUndrawnAndForeignPortals()
    {
        Assert.True(VmAmMapFakeFlat1134Slice.SkipSubsector(VmAmMapFakeFlat1134Slice.SubsectorPolyOrg, 0, 5, false, 1, 1));
        Assert.True(VmAmMapFakeFlat1134Slice.SkipSubsector(0, 0, 0, false, 0, 0));
        Assert.False(VmAmMapFakeFlat1134Slice.SkipSubsector(VmAmMapFakeFlat1134Slice.SubsectorDrawn, 0, 0, false, 0, 0));
        Assert.True(VmAmMapFakeFlat1134Slice.SkipSubsector(
            VmAmMapFakeFlat1134Slice.SubsectorDrawn, VmAmMapFakeFlat1134Slice.SectorHidden, 4, false, 0, 0));
        Assert.True(VmAmMapFakeFlat1134Slice.SkipSubsector(
            VmAmMapFakeFlat1134Slice.SubsectorDrawn, 0, 1, true, 3, 1));
        Assert.False(VmAmMapFakeFlat1134Slice.SkipSubsector(
            VmAmMapFakeFlat1134Slice.SubsectorDrawn, 0, 1, true, 0, 1));
    }
}
