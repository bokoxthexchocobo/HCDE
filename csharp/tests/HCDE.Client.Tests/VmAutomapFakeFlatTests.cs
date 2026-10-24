namespace HCDE.Client.Tests;

public class VmAutomapFakeFlatTests
{
    [Fact]
    public void ShouldPassThrough_WithoutHeightSec()
    {
        var input = new VmAutomapFakeFlat.FakeFlatInput(
            sectorHasHeightSec: false,
            viewerSectorHasHeightSec: false,
            viewZ: 40,
            heightSecFloorZAtView: 0,
            heightSecCeilingZAtView: 128,
            heightSecFlags: VmAutomapFakeFlat.HeightSecFlags.None);
        Assert.True(VmAutomapFakeFlat.ShouldPassThrough(input));
        Assert.Equal(VmAutomapFakeFlat.FakeFlatOutcome.UseOriginalSector, VmAutomapFakeFlat.PlanFakeFlat(input));
    }

    [Fact]
    public void ResolveViewerArea_BelowFloor_IsNegativeOne()
    {
        var input = new VmAutomapFakeFlat.FakeFlatInput(
            true,
            true,
            viewZ: 10,
            heightSecFloorZAtView: 32,
            heightSecCeilingZAtView: 128,
            VmAutomapFakeFlat.HeightSecFlags.None);
        Assert.Equal(VmAutomapFakeFlat.FakeFlatArea.Below, VmAutomapFakeFlat.ResolveViewerArea(input));
    }

    [Fact]
    public void PlanFakeFlat_FakeFloorOnly_AboveUsesOriginal()
    {
        var input = new VmAutomapFakeFlat.FakeFlatInput(
            true,
            true,
            viewZ: 200,
            heightSecFloorZAtView: 0,
            heightSecCeilingZAtView: 128,
            VmAutomapFakeFlat.HeightSecFlags.ClipFakePlanes | VmAutomapFakeFlat.HeightSecFlags.FakeFloorOnly);
        Assert.Equal(
            VmAutomapFakeFlat.FakeFlatOutcome.UseOriginalAfterFakeFloorOnlyBelow,
            VmAutomapFakeFlat.PlanFakeFlat(input));
    }

    [Fact]
    public void PickFloorTextureSource_Above_NonSkyUsesHeightSecFloor()
    {
        var tex = VmAutomapFakeFlat.PickFloorTextureSource(
            VmAutomapFakeFlat.FakeFlatArea.Above,
            diffTex: true,
            ceilingIsSkyFlat: false,
            sectorFloorTex: 1,
            heightSecFloorTex: 2,
            heightSecCeilingTex: 3);
        Assert.Equal(2, tex);
    }

    [Fact]
    public void ShouldApplyFakeLight_RespectsNoFakeLightFlag()
    {
        Assert.False(VmAutomapFakeFlat.ShouldApplyFakeLight(VmAutomapFakeFlat.HeightSecFlags.NoFakeLight));
        Assert.True(VmAutomapFakeFlat.ShouldApplyFakeLight(VmAutomapFakeFlat.HeightSecFlags.ClipFakePlanes));
    }
}
