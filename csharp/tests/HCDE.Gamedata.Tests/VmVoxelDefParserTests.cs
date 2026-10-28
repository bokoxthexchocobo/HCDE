namespace HCDE.Gamedata.Tests;

public class VmVoxelDefParserTests
{
    [Fact]
    public void TryReadSpriteNames_StopsAtEquals()
    {
        var tokens = VmLockDefTokenize.Tokenize("TROO TROOA =");
        var i = 0;
        var sprites = new List<VmVoxelSpriteBinding>();
        var errors = new List<string>();
        Assert.True(VmVoxelDefParser.TryReadSpriteNames(tokens, ref i, sprites, errors, _ => 1));
        Assert.Equal(2, sprites.Count);
        Assert.Equal(0, sprites[1].Frame);
    }

    [Fact]
    public void TryReadOptions_ReadsScaleSpinAndOffsets()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ scale = 2.0 spin = -3 angleoffset = 10 xoffset = 1 overridepalette }");
        var i = 0;
        var opts = new VmVoxelOptions();
        var errors = new List<string>();
        Assert.True(VmVoxelDefParser.TryReadOptions(tokens, ref i, opts, errors));
        Assert.Equal(2, opts.Scale);
        Assert.Equal(-3, opts.DroppedSpin);
        Assert.Equal(100, opts.AngleOffsetDeg);
        Assert.Equal(1, opts.XOffset);
        Assert.True(opts.OverridePalette);
    }

    [Fact]
    public void ApplyVoxelToSprites_ExpandsAllFrames()
    {
        var bindings = new List<VmVoxelSpriteBinding> { new() { SpriteIndex = 5, Frame = 255 } };
        var hits = new List<(int, int)>();
        VmVoxelDefParser.ApplyVoxelToSprites(bindings, "def", (s, f, _) => hits.Add((s, f)));
        Assert.Equal(VmVoxelDefParser.MaxSpriteFrames, hits.Count);
        Assert.All(hits, h => Assert.Equal(5, h.Item1));
    }
}
