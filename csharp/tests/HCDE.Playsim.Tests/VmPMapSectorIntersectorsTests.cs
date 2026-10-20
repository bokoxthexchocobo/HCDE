namespace HCDE.Playsim.Tests;

public class VmPMapSectorIntersectorsTests
{
    [Fact]
    public void AdjustFloor_MidtexPassmobj() =>
        Assert.True(VmPMapSectorIntersectors.MoveMidtexForcesPassmobj(true));

    [Fact]
    public void AdjustFloor_BridgeSkipsSync() =>
        Assert.True(VmPMapSectorIntersectors.BridgeSkipsFloorSync(
            VmPMapSectorIntersectors.Mf4Actlikebridge));

    [Fact]
    public void Above_SkipsNoclip() =>
        Assert.True(VmPMapSectorIntersectors.SkipNoclip(
            VmPMapSectorIntersectors.MfNoclip));

    [Fact]
    public void Above_IntersectionBand() =>
        Assert.True(VmPMapSectorIntersectors.AboveIntersection(5, 0, 10));

    [Fact]
    public void Below_IntersectionBand() =>
        Assert.True(VmPMapSectorIntersectors.BelowIntersection(5, 10, 0));

    [Fact]
    public void Crunch_CompatSkipsCorpse() =>
        Assert.True(VmPMapSectorIntersectors.CrusherCompatSkipsCorpse(
            VmPMapSectorIntersectors.Compatf2DrCrusher, false,
            VmPMapSectorIntersectors.MfCorpse));

    [Fact]
    public void Crunch_DamageCadence() =>
        Assert.True(VmPMapSectorIntersectors.ApplyCrushDamageOnCadence(10, 0));
}
