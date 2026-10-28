namespace HCDE.Playsim.Tests;

public class VmPLnspecLineTextureTests
{
    [Fact]
    public void SectorRotation_PacksFloorAndCeilingAngles()
    {
        var rot = VmPLnspecLineTextureLineSpecials.SectorSetRotation(1, 90, 180);
        Assert.Equal(90, rot.FloorDegrees);
        Assert.Equal(180, rot.CeilingDegrees);
    }

    [Fact]
    public void LineAlign_CeilingUsesTopPlane()
    {
        Assert.Equal(1, VmPLnspecLineTextureLineSpecials.LineAlignPlane(true));
    }

    [Fact]
    public void TextureOffset_RejectsInvalidSide()
    {
        Assert.True(VmPLnspecLineTextureLineSpecials.LineTextureReject(1, 2));
    }

    [Fact]
    public void TextureScale_Uses65536Factors()
    {
        Assert.Equal(1.0, VmPLnspecLineTextureLineSpecials.TextureArgFrom65536(65536));
    }

    [Fact]
    public void Blocking_RejectsZeroLineId()
    {
        Assert.True(VmPLnspecLineTextureLineSpecials.BlockingRejectZeroId(0));
    }
}
