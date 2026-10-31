namespace HCDE.Playsim.Tests;

public class VmSwRenderer1275SliceTests
{
    [Fact]
    public void Precache_OverwritesSpriteHitsAndLeavesAFlatOnPixels()
    {
        var sw = new VmSwRenderer1275();
        sw.SetOldFreelook(true);
        Assert.Empty(sw.Log);
        Assert.True(sw.OldFreelook);
        sw.UserGame = true;
        sw.SetOldFreelook(false);
        sw.SetOldFreelook(true);
        sw.Init();
        sw.Create();
        Assert.Equal(
"""
pitch 0
pitch 1
shade
colormaps
create
""",
            string.Join('\n', sw.Log));

        sw.Log.Clear();
        sw.TrueColorValue = true;
        sw.Prepare(null, 4);
        Assert.Equal(1, sw.TrueColorReads);
        sw.Prepare(new VmSwTex1275 { Valid = false }, 4);
        sw.Prepare(new VmSwTex1275 { SoftwareCanvas = true }, 4);
        sw.Prepare(new VmSwTex1275 { CheckPixels = true }, 0);
        sw.Prepare(new VmSwTex1275 { CheckPixels = true }, 8);
        sw.TrueColorValue = false;
        sw.Prepare(new VmSwTex1275 { CheckPixels = false }, 8);
        Assert.Equal(7, sw.TrueColorReads);
        Assert.Equal(
"""
unload
register 0
""",
            string.Join('\n', sw.Log));

        sw.Log.Clear();
        sw.TrueColorReads = 0;
        sw.TrueColorValue = true;
        sw.PrecacheTexture(null, 8);
        sw.PrecacheTexture(new VmSwTex1275(), 8);
        sw.PrecacheTexture(new VmSwTex1275(), 2);
        sw.PrecacheTexture(new VmSwTex1275(), 0);
        sw.TrueColorValue = false;
        sw.PrecacheTexture(new VmSwTex1275(), 1);
        sw.PrecacheTexture(new VmSwTex1275(), 13);
        sw.PrecacheTexture(new VmSwTex1275(), 2);
        Assert.Equal(
"""
column bgra
pixels bgra
column pal
column pal
pixels pal
""",
            string.Join('\n', sw.Log));

        var full = new VmSwRenderer1275();
        var frame0 = new VmSwFrame1275();
        frame0.Valid[0] = true;
        frame0.Index[0] = 2;
        frame0.Valid[3] = true;
        frame0.Index[3] = 0;
        var frame1 = new VmSwFrame1275();
        frame1.Valid[1] = true;
        frame1.Index[1] = 1;
        full.Frames.Add(frame0);
        full.Frames.Add(frame1);
        full.Sprites.Add(new VmSwSprite1275 { NumFrames = 1, FrameIndex = 0 });
        full.Sprites.Add(new VmSwSprite1275 { NumFrames = 1, FrameIndex = 1 });
        full.Sprites.Add(new VmSwSprite1275 { NumFrames = 1, FrameIndex = 0 });
        full.Textures.Add(new VmSwTex1275 { CheckPixels = true });
        full.Textures.Add(new VmSwTex1275 { CheckPixels = false });
        full.Textures.Add(null);
        full.Textures.Add(new VmSwTex1275 { CheckPixels = false });
        var hit = new[] { 2, 0, 1, 2 };
        Assert.Throws<IndexOutOfRangeException>(() =>
            full.Precache(hit, new[] { new VmSwClass1275 { StateSprites = [3] } }));
        Assert.Equal(new[] { 2, 0, 1, 2 }, hit);
        full.Precache(hit, new[]
        {
            new VmSwClass1275 { StateSprites = [0, 2] },
            new VmSwClass1275 { StateSprites = [1] },
        });
        Assert.Equal(new[] { 8, 8, 8, 2 }, hit);
        Assert.Equal(10, full.TrueColorReads);
        Assert.Equal(
"""
mark 2 0 0 2
mark 2 0 3 0
mark 1 0 1 1
mark 0 0 0 2
mark 0 0 3 0
begin
register 0
register 0
pixels pal
column pal
column pal
end
""",
            string.Join('\n', full.Log));
    }

    [Fact]
    public void Views_RestoreAfterTheDrawAndSkipAMissingCamera()
    {
        var view = new VmSwRenderer1275
        {
            Pitch = 10,
            Window = 4,
            Sector = 6,
            Fov = 90,
            ScenePitchAfter = 11,
            SceneWindowAfter = 5,
            SavePitchAfter = 12,
            SaveWindowAfter = 6,
            RemainPitchAfter = 13,
            RemainWindowAfter = 7,
            CameraPitchAfter = 14,
            CameraWindowAfter = 8,
            RenderTargetBgra = true,
            LightToken = 3,
        };
        view.RenderView(new[]
        {
            new VmSwCam1275 { Id = 1, Present = false, Width = 32, Height = 16, Fov = 40 },
            new VmSwCam1275 { Id = 2, Present = true, Width = 64, Height = 32, Fov = 50, Actor = 9, FirstUpdate = true },
        });
        view.RenderTargetBgra = false;
        view.WriteSavePic(null, 320, 200);
        view.DrawRemaining();
        view.SetClearColor(7);
        view.SetColormap(4, 0, "custom", "fade");
        view.SetColormap(0, 0, "custom", "fade");
        view.SetColormap(0, 9, "custom", "fade");
        Assert.Equal(90, view.Fov);
        Assert.Equal(13, view.Pitch);
        Assert.Equal(7, view.Window);
        Assert.Equal(
"""
install 10 4
renderview
restore 11 5
cam install 11 5
cam missing 1
cam install 11 5
canvas bgra 64 32
light save 3
fov 50
tocanvas 9 0 0 64 32 1
fov 90
pixels 1
light restore 3
cam restore 14 8
install 14 8
tocanvas null 0 0 320 200
restore 12 6
write 0 320 200 6 0
install 12 6
playersprites
restore 13 7
clear 7
maps
color 255 255 255 0
fade 0
cmap custom
maps
color 255 255 255 0
fade 0
cmap fade
maps
color 255 255 255 0
fade 9
""",
            string.Join('\n', view.Log));
    }
}
