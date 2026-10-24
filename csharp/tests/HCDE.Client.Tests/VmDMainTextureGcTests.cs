namespace HCDE.Client.Tests;

public class VmDMainTextureGcTests
{
    [Fact]
    public void PatchTextures_OnlyHexenGametype() =>
        Assert.True(VmDMainTextureGc.PatchTexturesApplies(2));

    [Fact]
    public void CheckForHacks_FixesHereticSkyHeight()
    {
        var k = VmDMainTextureGc.ClassifyBuildInfo(new(3, 0, "SKY2", 128, 1, 0));
        Assert.Equal(VmDMainTextureGc.TextureHackKind.HereticSky, k);
    }

    [Fact]
    public void CheckForHacks_FixesDoomSkyOrigin()
    {
        var k = VmDMainTextureGc.ClassifyBuildInfo(new(1, 0, "SKY1", 128, 1, -8));
        Assert.Equal(VmDMainTextureGc.TextureHackKind.DoomSky1, k);
    }

    [Fact]
    public void FixWideStatusBar_CentersWhenWide()
    {
        var p = VmDMainTextureGc.PlanWideStatusBar(400, 1, 0, 0);
        Assert.Equal(40, p.Offset0);
    }

    [Fact]
    public void GcMark_IncludesNextToThink() =>
        Assert.True(VmDMainTextureGc.ShouldMarkNextThink(new object()));

    [Fact]
    public void HudScale_SetsSizeNeededWhenBarPresent() =>
        Assert.True(VmDMainTextureGc.HudScaleNeedsResize(new object()));

    [Fact]
    public void FileNameCheck_RejectsLz4TexturesFile() =>
        Assert.True(VmDMainTextureGc.FileNameRejectLz4Textures("textures", "LZ4"u8));
}
