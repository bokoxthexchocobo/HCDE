namespace HCDE.Playsim.Tests;

public class VmPLnspecRotationAlign930SliceTests
{
    [Fact]
    public void SectorRotation_PacksFloorAndCeilingAngles()
    {
        var rot = VmPLnspecRotationAlign930Slice.SectorSetRotation(1, 90, 180);
        Assert.Equal(90, rot.FloorDegrees);
        Assert.Equal(180, rot.CeilingDegrees);
    }

    [Fact]
    public void LineAlign_CeilingUsesTopPlane()
    {
        Assert.Equal(1, VmPLnspecRotationAlign930Slice.LineAlignFlatPlane(true));
        Assert.Equal(0, VmPLnspecRotationAlign930Slice.LineAlignFlatPlane(false));
    }

    [Fact]
    public void TextureOffset_RejectsInvalidSide()
    {
        Assert.True(VmPLnspecRotationAlign930Slice.LineTextureReject(1, 2));
    }

    [Fact]
    public void TextureScale_Uses65536Factors()
    {
        Assert.Equal(1.0, VmPLnspecRotationAlign930Slice.TextureArgFrom65536(65536));
    }

    [Fact]
    public void BuildTextureOffset_DetectsAddMode()
    {
        var invoke = VmPLnspecRotationAlign930Slice.BuildTextureOffset(5, 65536, 0, 1, 8);
        Assert.False(invoke.SetMode);
        Assert.True(VmPLnspecRotationAlign930Slice.UsesMultiplyScaleMode(8));
    }

    [Fact]
    public void CombineLineAlignResults_OrAccumulates()
    {
        Assert.True(VmPLnspecRotationAlign930Slice.CombineLineAlignResults(false, true));
    }

    [Fact]
    public void CountAffectedTextureParts_CountsFlagBits()
    {
        Assert.Equal(2, VmPLnspecRotationAlign930Slice.CountAffectedTextureParts(3));
    }

    [Fact]
    public void TextureAxisApplyMask_SkipsNoChangeRaw()
    {
        var (applyX, applyY) = VmPLnspecRotationAlign930Slice.TextureAxisApplyMask(
            VmPLnspecRotationAlign930Slice.TextureNoChange, 65536, 7);
        Assert.False(applyX);
        Assert.True(applyY);
    }
}
