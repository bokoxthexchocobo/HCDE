namespace HCDE.Gamedata.Tests;

public class VmKeyConfPlayerParserTests
{
    [Fact]
    public void ClearPlayerClasses_StrictClearsAll()
    {
        var list = new List<VmKeyConfPlayerClassEntry> { new() { ClassName = "Foo" } };
        VmKeyConfPlayerParser.ClearPlayerClasses(list, setSlotStrict: true);
        Assert.Empty(list);
    }

    [Fact]
    public void ClearPlayerClasses_NonStrictRemovesIwadNames()
    {
        var list = new List<VmKeyConfPlayerClassEntry>
        {
            new() { ClassName = "DoomPlayer" },
            new() { ClassName = "CustomGuy" }
        };
        VmKeyConfPlayerParser.ClearPlayerClasses(list, setSlotStrict: false);
        Assert.Single(list);
        Assert.Equal("CustomGuy", list[0].ClassName);
    }

    [Fact]
    public void TryAddPlayerClass_DeduplicatesAndParsesNomenu()
    {
        var list = new List<VmKeyConfPlayerClassEntry>();
        var errors = new List<string>();
        Assert.True(VmKeyConfPlayerParser.TryAddPlayerClass(list, "Custom", ["nomenu"], errors, _ => true));
        Assert.True(list[0].NoMenu);
        Assert.True(VmKeyConfPlayerParser.TryAddPlayerClass(list, "Custom", [], errors, _ => true));
        Assert.Single(list);
    }
}
