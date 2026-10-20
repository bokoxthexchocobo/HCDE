namespace HCDE.Gamedata.Tests;

public class VmSpriteInitScanTests
{
    [Fact]
    public void SkinSpriteNameToInt_PacksFourChars()
    {
        Assert.Equal(VmSpriteTextureHasher.SpriteNameId("PLAY"), VmSpriteInitScan.SkinSpriteNameToInt("play"));
    }

    [Fact]
    public void ScanSpriteTextures_InvokesInstall()
    {
        var id = VmSpriteTextureHasher.SpriteNameId("TROO");
        var bucket = (int)(id % 2);
        var head = new List<int> { -1, -1 };
        head[bucket] = 0;
        var next = new List<int> { -1, -1 };
        var textures = new List<(uint, string)> { (id, "TROOA1") };
        var frames = new List<VmSpriteTempFrame> { new() };
        var calls = 0;
        var maxFrame = -1;
        VmSpriteInitScan.ScanSpriteTextures(id, head, next, textures, frames, ref maxFrame, (_, _, _, _, _) => calls++);
        Assert.Equal(1, calls);
    }
}
