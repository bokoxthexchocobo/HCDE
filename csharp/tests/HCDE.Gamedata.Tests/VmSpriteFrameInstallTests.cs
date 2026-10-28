namespace HCDE.Gamedata.Tests;

public class VmSpriteFrameInstallTests
{
    [Fact]
    public void InstallSpriteLump_FillsAllRotationsForZeroRotation()
    {
        var frame = new VmSpriteTempFrame();
        var max = -1;
        Assert.True(VmSpriteFrameInstall.InstallSpriteLump(frame, 5, 0, '0', false, ref max));
        Assert.Equal(5, frame.TextureIds[0]);
        Assert.Equal(0, frame.RotateMode);
    }

    [Fact]
    public void FinalizeSpriteFrames_PropagatesSingleRotation()
    {
        var frames = new List<VmSpriteTempFrame> { new() };
        frames[0].TextureIds[0] = 1;
        frames[0].RotateMode = 0;
        var max = 0;
        VmSpriteFrameInstall.FinalizeSpriteFrames(frames, ref max);
        Assert.Equal(1, frames[0].TextureIds[15]);
    }

    [Fact]
    public void TryGetSpriteTextureId_UsesRequestedRotationAndMirror()
    {
        var frame = new VmSpriteTempFrame();
        for (var i = 0; i < 16; i++) frame.TextureIds[i] = i + 10;
        frame.FlipMask = 1 << 3;
        Assert.True(VmSpriteFrameInstall.TryGetSpriteTextureId(frame, 0, 1, 3, 0, false, out var tex, out var mirror));
        Assert.Equal(13, tex);
        Assert.True(mirror);
    }

    [Fact]
    public void InstallSpriteLump_AssignsSpecificRotation()
    {
        var frame = new VmSpriteTempFrame();
        var max = -1;
        Assert.True(VmSpriteFrameInstall.InstallSpriteLump(frame, 9, 0, '2', true, ref max));
        Assert.Equal(9, frame.TextureIds[2]);
        Assert.Equal(1, frame.RotateMode);
        Assert.NotEqual(0, frame.FlipMask & (1 << 2));
    }
}
