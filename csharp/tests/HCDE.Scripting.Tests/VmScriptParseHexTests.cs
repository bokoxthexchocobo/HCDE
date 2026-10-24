namespace HCDE.Scripting.Tests;

public class VmScriptParseHexTests
{
    [Fact]
    public void Parse_DecodesHexDigits() => Assert.Equal(0x1A, VmScriptParseHex.Parse("1a"));
}
