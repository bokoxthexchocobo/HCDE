namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchLexerTests
{
    [Fact]
    public void ReplaceSpecialChars_DecodesNewlineAndHex()
    {
        Assert.Equal("A\nB", VmDehackedPatchLexer.ReplaceSpecialChars("A\\nB"));
        Assert.Equal("\x0A", VmDehackedPatchLexer.ReplaceSpecialChars("\\x0A"));
    }

    [Fact]
    public void TryParsePatchLine_SplitsKeyValue()
    {
        Assert.True(VmDehackedPatchLexer.TryParsePatchLine("Hit points = 100", out var k, out var v, out var kind));
        Assert.Equal("Hit points", k);
        Assert.Equal("100", v);
        Assert.Equal(VmDehackedPatchLexer.PatchLineKind.KeyValue, kind);
    }

    [Fact]
    public void ReadChars_SkipsCarriageReturns()
    {
        var patch = "ab\rcd";
        var offset = 0;
        Assert.Equal("abcd", VmDehackedPatchLexer.ReadChars(patch, ref offset, 4));
    }
}
