namespace HCDE.Playsim.Tests;

public class VmP3dFloorsLightOpening959SliceTests
{
    [Fact]
    public void RefreshLightListEntry_UsesSourceBlendWhenPresent()
    {
        var colormap = new object();
        object? extra = null;
        var blend = -1;
        VmP3dFloorsLightOpening959Slice.RefreshLightListEntry(
            new object(),
            colormap,
            (_, _) => { },
            _ => 42,
            v => extra = v,
            v => blend = v);
        Assert.Equal(42, blend);
    }

    [Fact]
    public void RefreshLightListEntry_FallsBackToSectorColormap()
    {
        var sector = "sector";
        object? extra = null;
        var blend = -1;
        VmP3dFloorsLightOpening959Slice.RefreshLightListEntry(
            null,
            sector,
            (_, _) => { },
            _ => 1,
            v => extra = v,
            v => blend = v);
        Assert.Same(sector, extra);
        Assert.Equal(0, blend);
    }

    [Fact]
    public void RecalculateAttachedSectorLights_VisitsAttachedBeforeSelf()
    {
        var order = new List<int>();
        VmP3dFloorsLightOpening959Slice.RecalculateAttachedSectorLights(
            2,
            order.Add,
            () => order.Add(99));
        Assert.Equal(new[] { 0, 1, 99 }, order);
    }

    [Fact]
    public void PlaneHeightAtCenter_SubtractsEpsilonOnUnderside()
    {
        var height = VmP3dFloorsLightOpening959Slice.PlaneHeightAtCenter(
            (_, _) => 10,
            0,
            0,
            underside: true);
        Assert.Equal(10 - VmP3dFloorsLightOpening959Slice.EqualEpsilon, height);
    }

    [Fact]
    public void SelectPlaneLightIndex_PicksTierBelowPlane()
    {
        var idx = VmP3dFloorsLightOpening959Slice.SelectPlaneLightIndex(
            3,
            i => i switch { 1 => 110, 2 => 90, _ => 0 },
            planeHeight: 100);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void Solid3dFloorActive_RequiresExistsAndSolid()
    {
        Assert.True(VmP3dFloorsLightOpening959Slice.Solid3dFloorActive(
            VmP3dFloorsLightOpening959Slice.FfExists | VmP3dFloorsLightOpening959Slice.FfSolid));
        Assert.False(VmP3dFloorsLightOpening959Slice.Solid3dFloorActive(
            VmP3dFloorsLightOpening959Slice.FfExists));
    }

    [Fact]
    public void ShouldPickLowestCeiling_WhenBottomCloserToThingFeet()
    {
        var (d1, d2) = VmP3dFloorsLightOpening959Slice.MidpointDistanceDeltas(0, 56, 20, 40);
        Assert.True(VmP3dFloorsLightOpening959Slice.ShouldPickLowestCeiling(20, 100, d1, d2));
    }

    [Fact]
    public void FloorCandidateDominatesThing_RespectsRestrictFlag()
    {
        Assert.False(VmP3dFloorsLightOpening959Slice.FloorCandidateDominatesThing(1, 2, true, 10, 20));
        Assert.True(VmP3dFloorsLightOpening959Slice.FloorCandidateDominatesThing(2, 2, true, 25, 20));
    }

    [Fact]
    public void ShouldTrackSteppableLowestFloor_WithinMaxStep()
    {
        Assert.True(VmP3dFloorsLightOpening959Slice.ShouldTrackSteppableLowestFloor(30, 10, 20, 24));
    }

    [Fact]
    public void SecPlaneNeedsFlip_WhenCoeffNegative()
    {
        Assert.True(VmP3dFloorsLightOpening959Slice.SecPlaneNeedsFlip(-1));
    }
}
