namespace HCDE.Client.Tests;

public class VmStringTableTests
{
    [Fact]
    public void LoadAndLookup_ReturnsInsertedString()
    {
        var table = new VmStringTable();
        table.LoadLanguage(1, """
            [default]
            HELLO = world ;
            """);
        table.UpdateLanguage("en-US");
        Assert.Equal("world", table.GetString("HELLO"));
        Assert.True(table.Exists("HELLO"));
    }

    [Fact]
    public void CheckString_FollowsDollarAlias()
    {
        var table = new VmStringTable();
        table.LoadLanguage(1, """
            [default]
            A = x ;
            B = $$A ;
            """);
        table.UpdateLanguage("en-US");
        Assert.Equal("x", table.CheckString("B", out _));
    }

    [Fact]
    public void ProcessEscapes_DecodesNewline()
    {
        Assert.Equal("a\nb", VmStringTableEscapes.Process("a\\nb"));
    }
}
