namespace HCDE.Playsim.Tests;

public class VmPLnspecPolyobjLineSpecialsTests
{
    [Fact]
    public void ModToDamageType_Telefrag() =>
        Assert.Equal("Telefrag", VmPLnspecPolyobjLineSpecials.ModToDamageTypeName(16));

    [Fact]
    public void CeilingCrushType_HexenDefault() =>
        Assert.Equal(
            VmPLnspecPolyobjLineSpecials.CrushMode.Hexen,
            VmPLnspecPolyobjLineSpecials.CeilingCrushType(0, false, VmPLnspecPolyobjLineSpecials.GameHexen));

    [Fact]
    public void MoveTimes8_MultipliesDistance() =>
        Assert.Equal(40, VmPLnspecPolyobjLineSpecials.PackMoveTimes8(1, 8, 128, 5, false).Distance);

    [Fact]
    public void MoveToSpot_FailsWithoutActor() =>
        Assert.False(VmPLnspecPolyobjLineSpecials.MoveToSpotRequiresActor(null));

    [Fact]
    public void DoorSlide_UsesSlideKind() =>
        Assert.Equal(
            VmPLnspecPolyobjLineSpecials.PodoorSlide,
            VmPLnspecPolyobjLineSpecials.PackDoorSlide(1, 4, 64, 32, 8).Kind);

    [Fact]
    public void Nop_ReturnsFalse() =>
        Assert.False(VmPLnspecPolyobjLineSpecials.NopReturnsFalse());
}
