namespace HCDE.Playsim.Tests;

public class VmGLevelCvarHooks1010SliceTests
{
    [Fact]
    public void SaveChunkIds()
    {
        Assert.Equal(0x70416E73, VmGLevelCvarHooks1010Slice.SnapId);
        Assert.Equal(0x64536361, VmGLevelCvarHooks1010Slice.AcsdId);
    }

    [Fact]
    public void GlCvarPropagation()
    {
        var level = new VmGLevelCvarHooks1010Slice.LevelModel
        {
            Info = new VmGLevelCvarHooks1010Slice.LevelInfoModel { BrightFog = -1 },
        };
        VmGLevelCvarHooks1010Slice.ApplyBrightFogToLevel(level, value: true);
        Assert.True(level.BrightFog);
        Assert.False(VmGLevelCvarHooks1010Slice.ShouldApplyLevelInfoDefault(infoField: 0));
        VmGLevelCvarHooks1010Slice.PropagateLightAdditiveSurfaces(
            [level],
            value: true);
        Assert.True(level.LightAdditiveSurfaces);
    }

    [Fact]
    public void GlLightModeClamps()
    {
        Assert.Equal(-1, VmGLevelCvarHooks1010Slice.ClampGlMapLightMode(99));
        Assert.Equal(1, VmGLevelCvarHooks1010Slice.ClampGlLightMode(-1));
        Assert.Equal(2, VmGLevelCvarHooks1010Slice.ClampGlLightMode(2));
    }
}
