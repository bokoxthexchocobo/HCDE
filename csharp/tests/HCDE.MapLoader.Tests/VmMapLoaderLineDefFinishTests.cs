namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLineDefFinishTests
{
    [Fact]
    public void ResolveHexenLineId_SetIdentification_UsesArg0WhenHack()
    {
        var id = VmMapLoaderLineDefFinish.ResolveHexenLineId(
            VmMapLoaderLineDefFinish.MapTypeHexen,
            (int)VmMapLoaderLineDefFinish.HexenLineSpecial.SetIdentification,
            arg0: 7,
            arg1: 0,
            arg2: 0,
            arg3: 0,
            arg4: 3,
            levelHexenHack: true,
            noSlopeIdCompat: false);
        Assert.Equal(7, id);
    }

    [Fact]
    public void ResolveHexenLineId_PlaneAlign_RespectsNoSlopeCompat()
    {
        Assert.Null(VmMapLoaderLineDefFinish.ResolveHexenLineId(
            VmMapLoaderLineDefFinish.MapTypeHexen,
            (int)VmMapLoaderLineDefFinish.HexenLineSpecial.PlaneAlign,
            0, 0, 5, 0, 0,
            levelHexenHack: false,
            noSlopeIdCompat: true));
    }

    [Fact]
    public void ShouldSaveLineSpecial_StaticInitColorOnly()
    {
        Assert.True(VmMapLoaderLineDefFinish.ShouldSaveLineSpecial(true, 8, 2));
        Assert.False(VmMapLoaderLineDefFinish.ShouldSaveLineSpecial(true, 8, 0));
    }

    [Fact]
    public void ResolveTranslucentLine_NegativeAlpha_IsAdditive()
    {
        var t = VmMapLoaderLineDefFinish.ResolveTranslucentLine(2, 0, 0, -128);
        Assert.NotNull(t);
        Assert.True(t!.Value.Additive);
        Assert.Equal(128 / 255.0, t!.Value.Alpha, 3);
    }

    [Fact]
    public void ComputeTexelLength_RoundsHypotenuse()
    {
        Assert.Equal(5, VmMapLoaderLineDefFinish.ComputeTexelLength(3, 4));
    }
}
