namespace HCDE.Gamedata.Tests;

public class VmAnimDefExtendedClauseTests
{
    private sealed class Catalog : IAnimDefTextureCatalog
    {
        private readonly HashSet<string> _names;
        public Catalog(params string[] names) => _names = new(names, StringComparer.OrdinalIgnoreCase);
        public bool TextureExists(string name, bool isFlat) => _names.Contains(name);
        public int GetTextureIndex(string name, bool isFlat) => TextureExists(name, isFlat) ? 1 : 0;
    }

    [Fact]
    public void CameraTexture_ParsesDimensionsAndFlags()
    {
        var tokens = VmLockDefTokenize.Tokenize("CAM 256 128 hdr fit 128 64 WorldPanning translucent");
        var i = 0;
        var camera = new VmAnimDefCameraTexture();
        var errors = new List<string>();
        Assert.True(VmAnimDefCameraParser.TryParseCamera(tokens, ref i, new Catalog("CAM"), camera, errors), string.Join("; ", errors));
        Assert.Equal("CAM", camera.Name);
        Assert.Equal(256, camera.Width);
        Assert.True(camera.Hdr);
        Assert.Equal(128, camera.FitWidth);
        Assert.True(camera.WorldPanning);
        Assert.True(camera.Translucent);
    }

    [Fact]
    public void AnimatedDoor_ParsesSoundsAndPics()
    {
        var tokens = VmLockDefTokenize.Tokenize("DOOR1 opensound open1 closesound close1 pic FRAME1");
        var i = 0;
        var door = new VmAnimDefDoor();
        var errors = new List<string>();
        Assert.True(VmAnimDefDoorParser.TryParseDoor(tokens, ref i, new Catalog("DOOR1", "FRAME1"), door, errors), string.Join("; ", errors));
        Assert.Equal("open1", door.OpenSound);
        Assert.Equal("FRAME1", door.FrameTextures[0]);
    }

    [Fact]
    public void FireTexture_ParsesTicsAndColors()
    {
        var tokens = VmLockDefTokenize.Tokenize("FIRE tics 35 color 255 0 0 255");
        var i = 0;
        var fire = new VmAnimDefFireTexture();
        var errors = new List<string>();
        Assert.True(VmAnimDefFireParser.TryParseFire(tokens, ref i, fire, errors), string.Join("; ", errors));
        Assert.True(fire.DurationMs >= 900);
        Assert.Single(fire.Colors);
    }

    [Fact]
    public void SkyOffset_RequiresTextureAndOffset()
    {
        var tokens = VmLockDefTokenize.Tokenize("skyoffset SKY1 42");
        var i = 0;
        var entry = new VmAnimDefEntry();
        var errors = new List<string>();
        Assert.True(VmAnimDefDocumentParser.TryParseStatement(tokens, ref i, new Catalog("SKY1"), entry, errors), string.Join("; ", errors));
    }
}
