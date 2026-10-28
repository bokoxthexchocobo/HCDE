namespace HCDE.Gamedata.Tests;

public class VmKeyConfParserTests
{
    [Fact]
    public void StripKeyConfComments_RemovesLineAndBlockComments()
    {
        Assert.Equal("addkeysection Weapons keys", VmKeyConfParser.StripKeyConfComments("addkeysection Weapons keys // tail"));
        Assert.Equal("cmd", VmKeyConfParser.StripKeyConfComments("c/*block*/md"));
    }

    [Fact]
    public void TryAddKeySection_DeduplicatesByIniNameAgainstTitle()
    {
        var sections = new List<VmKeyConfSection>();
        var cur = -1;
        Assert.True(VmKeyConfParser.TryAddKeySection(sections, ref cur, "keys", "weapons", out var created));
        Assert.True(created);
        Assert.True(VmKeyConfParser.TryAddKeySection(sections, ref cur, "Other", "keys", out created));
        Assert.False(created);
        Assert.Equal(0, cur);
        Assert.Single(sections);
    }

    [Fact]
    public void TryAddMenuKey_RequiresSection()
    {
        var sections = new List<VmKeyConfSection>();
        var errors = new List<string>();
        Assert.False(VmKeyConfParser.TryAddMenuKey(sections, -1, "desc", "cmd", errors));
        Assert.Contains("addkeysection", errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseKeyConfLumpCommands_StripsCommentsAcrossLines()
    {
        var lump = "addkeysection A sec\n// comment only\naddmenukey d c /* inline */ end";
        var lines = VmKeyConfParser.ParseKeyConfLumpCommands(lump);
        Assert.Equal(2, lines.Count);
        Assert.Equal("addkeysection A sec", lines[0]);
        Assert.Equal("addmenukey d c  end", lines[1]);
    }

    [Fact]
    public void BuildBindingsSectionName_MatchesNativePattern()
    {
        Assert.Equal("Doom.keys.Bindings", VmKeyConfParser.BuildBindingsSectionName("Doom", "keys", false));
        Assert.Equal("Doom.keys.DoubleBindings", VmKeyConfParser.BuildBindingsSectionName("Doom", "keys", true));
    }
}
