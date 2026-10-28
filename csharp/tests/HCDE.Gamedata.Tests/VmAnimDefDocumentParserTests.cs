namespace HCDE.Gamedata.Tests;

public class VmAnimDefDocumentParserTests
{
    private sealed class Catalog : IAnimDefTextureCatalog
    {
        private readonly Dictionary<string, int> _names;
        public Catalog(params string[] names)
        {
            _names = new(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < names.Length; i++) _names[names[i]] = i + 1;
        }

        public bool TextureExists(string name, bool isFlat) => _names.ContainsKey(name);
        public int GetTextureIndex(string name, bool isFlat) => _names.GetValueOrDefault(name);
    }

    [Fact]
    public void FlatOptional_MarksMissingWithoutError()
    {
        var tokens = VmLockDefTokenize.Tokenize("flat optional NOPE");
        var i = 0;
        var entry = new VmAnimDefEntry();
        var errors = new List<string>();
        Assert.True(VmAnimDefDocumentParser.TryParseStatement(tokens, ref i, new Catalog("FLOOR1"), entry, errors), string.Join("; ", errors));
        Assert.True(entry.BaseMissing);
        Assert.True(entry.Optional);
    }

    [Fact]
    public void RandomAndOscillate_AreRejectedTogether()
    {
        var entry = new VmAnimDefEntry { AnimationType = "OscillateUp" };
        var tokens = VmLockDefTokenize.Tokenize("Random");
        var i = 0;
        var errors = new List<string>();
        Assert.False(VmAnimDefDocumentParser.TryParseAnimModifier(tokens, ref i, entry, errors));
        Assert.Contains("oscillate", errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PicAndRange_BuildFrames()
    {
        var entry = new VmAnimDefEntry { BaseTexture = "WALL1", Kind = "texture" };
        var tokens = VmLockDefTokenize.Tokenize("pic WALL2 tics 2.0 pic WALL3 tics 3.0");
        var i = 0;
        var errors = new List<string>();
        var catalog = new Catalog("WALL1", "WALL2", "WALL3");
        while (i < tokens.Count)
            Assert.True(VmAnimDefDocumentParser.TryParseAnimClause(tokens, ref i, catalog, entry, isFlat: false, errors), string.Join("; ", errors));
        Assert.Equal(2, entry.PicFrames.Count);
    }
}
