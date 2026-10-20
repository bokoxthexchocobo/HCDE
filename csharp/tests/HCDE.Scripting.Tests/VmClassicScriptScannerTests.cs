namespace HCDE.Scripting.Tests;

public class VmClassicScriptScannerTests
{
    [Fact]
    public void CheckString_MatchesCaseInsensitive()
    {
        var scanner = new VmClassicScriptScanner("actor Foo { }");
        Assert.True(scanner.CheckString("actor"));
        Assert.True(scanner.CheckString("Foo"));
    }

    [Fact]
    public void CheckString_UnGetsOnMismatch()
    {
        var scanner = new VmClassicScriptScanner("state See");
        Assert.False(scanner.CheckString("actor"));
        Assert.True(scanner.CheckString("state"));
    }

    [Fact]
    public void GetString_ParsesQuotedTokens()
    {
        var scanner = new VmClassicScriptScanner("name \"quoted value\" next");
        Assert.True(scanner.GetString());
        Assert.Equal("name", scanner.Current);
        Assert.True(scanner.GetString());
        Assert.Equal("quoted value", scanner.Current);
    }
}
