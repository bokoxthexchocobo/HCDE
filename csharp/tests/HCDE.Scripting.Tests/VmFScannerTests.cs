namespace HCDE.Scripting.Tests;

public class VmFScannerTests
{
    [Fact]
    public void CheckString_MatchesCaseInsensitive()
    {
        var scanner = new VmFScanner();
        scanner.OpenString("test", "actor Foo { }");
        Assert.True(scanner.CheckString("actor"));
        Assert.True(scanner.CheckString("Foo"));
    }

    [Fact]
    public void CheckString_UnGetsOnMismatch()
    {
        var scanner = new VmFScanner();
        scanner.OpenString("test", "state See");
        Assert.False(scanner.CheckString("actor"));
        Assert.True(scanner.CheckString("state"));
    }

    [Fact]
    public void GetNumber_ParsesHexAndMaxInt()
    {
        var scanner = new VmFScanner();
        scanner.OpenString("test", "0x10 MAXINT 5\n");
        Assert.True(scanner.GetNumber());
        Assert.Equal(16, scanner.Number);
        Assert.True(scanner.GetNumber());
        Assert.Equal(int.MaxValue, scanner.Number);
        Assert.True(scanner.GetNumber());
        Assert.Equal(5, scanner.Number);
    }

    [Fact]
    public void MustGetFloat_ReadsDecimal()
    {
        var scanner = new VmFScanner();
        scanner.OpenString("test", "1.25\n");
        scanner.MustGetFloat();
        Assert.Equal(1.25, scanner.Float);
    }

    [Fact]
    public void GetToken_RecognizesIfKeyword()
    {
        var scanner = new VmFScanner();
        scanner.OpenString("test", "if true;\n");
        scanner.SetCMode(true);
        Assert.True(scanner.GetToken());
        Assert.Equal(VmScriptTokens.If, scanner.TokenType);
    }
}
