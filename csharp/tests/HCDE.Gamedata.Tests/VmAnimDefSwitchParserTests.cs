namespace HCDE.Gamedata.Tests;

public class VmAnimDefSwitchParserTests
{
    private sealed class Catalog : IAnimDefTextureCatalog
    {
        public bool TextureExists(string name, bool isFlat) => true;
        public int GetTextureIndex(string name, bool isFlat) => 1;
    }

    [Fact]
    public void Switch_OnState_ParsesFrames()
    {
        var tokens = VmLockDefTokenize.Tokenize("any SW1BRCOM on { SW2BRCOM tics 1.0 }");
        var i = 0;
        var errors = new List<string>();
        Assert.True(VmAnimDefSwitchParser.TryParseSwitch(tokens, ref i, new Catalog(), errors), string.Join("; ", errors));
    }
}
