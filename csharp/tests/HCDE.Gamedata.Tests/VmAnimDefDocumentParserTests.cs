namespace HCDE.Gamedata.Tests;

public class VmAnimDefDocumentParserTests
{
    private sealed class Catalog : IAnimDefTextureCatalog
    {
        private readonly HashSet<string> _names;
        public Catalog(params string[] names) => _names = new(names, StringComparer.OrdinalIgnoreCase);
        public bool TextureExists(string name, bool isFlat) => _names.Contains(name);
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
}
