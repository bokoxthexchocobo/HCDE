namespace HCDE.Client.Tests;

public class VmDMainTextureGcTests
{
    [Fact]
    public void PatchTextures_OnlyHexenGametype()
    {
        Assert.True(VmDMainTextureGc.PatchTexturesApplies(2));
    }

    [Fact]
    public void CheckForHacks_FixesHereticSkyHeight()
    {
        Assert.True(VmDMainTextureGc.HereticSkyHack("SKY1", 128, 1));
    }

    [Fact]
    public void CheckForHacks_FixesDoomSkyOrigin()
    {
        Assert.True(VmDMainTextureGc.DoomSkyHack("SKY1", 128, -8, 0, 1));
    }

    [Fact]
    public void FixWideStatusBar_CentersWhenWide()
    {
        Assert.Equal(40, VmDMainTextureGc.WideBarOffset(400));
    }

    [Fact]
    public void GcMark_IncludesNextToThink()
    {
        Assert.True(VmDMainTextureGc.GcMarkNextThink(new object()));
    }

    [Fact]
    public void HudScale_SetsSizeNeededWhenBarPresent()
    {
        Assert.True(VmDMainTextureGc.HudScaleNeedsResize(new object()));
    }

    [Fact]
    public void FileNameCheck_RejectsLz4TexturesFile()
    {
        Assert.True(VmDMainTextureGc.FileNameCheckLz4Header("LZ4"u8));
    }
}
