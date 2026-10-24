namespace HCDE.MapLoader.Tests;

public class VmMapLoaderSectorInitTests
{
    [Fact]
    public void ResolveDefaultSeqType_TotalCtrl_IsZero()
    {
        Assert.Equal(0, VmMapLoaderSectorInit.ResolveDefaultSeqType(true));
        Assert.Equal(-1, VmMapLoaderSectorInit.ResolveDefaultSeqType(false));
    }

    [Fact]
    public void ShouldSetFloorDropFlag_OnlyWhenNoBehavior()
    {
        Assert.True(VmMapLoaderSectorInit.ShouldSetFloorDropFlag(false));
        Assert.False(VmMapLoaderSectorInit.ShouldSetFloorDropFlag(true));
    }

    [Fact]
    public void ResolveFadeColor_OutsideFog_WhenSkyCeiling()
    {
        var fade = VmMapLoaderSectorInit.ResolveFadeColor(
            0x00aabbccu,
            ceilingIsSky: true,
            specialLowByte: 0,
            levelHasFadeTable: false,
            levelFadeTo: 0x111111u);
        Assert.Equal(0x00aabbccu, fade);
    }
}
