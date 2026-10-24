namespace HCDE.Gamedata.Tests;

public class VmDecalLibDocumentParserTests
{
    [Fact]
    public void ParseDecal_AppliesShadeAndTranslatable()
    {
        var db = new VmDecalDatabase();
        var errors = new List<string>();
        Assert.True(VmDecalLibDocumentParser.TryParseDocument(
            "decal Blood 1 { shade BloodDefault translatable }", db, errors), string.Join("; ", errors));
        var def = Assert.Single(db.Decals);
        Assert.Equal("BloodDefault", def.Template.ShadeColor);
        Assert.True(def.Template.Translatable);
        Assert.Equal("Normal", def.Template.RenderStyle);
    }

    [Fact]
    public void ParseDecalGroup_RequiresDefinedDecals()
    {
        var db = new VmDecalDatabase();
        var errors = new List<string>();
        Assert.True(VmDecalLibDocumentParser.TryParseDocument("decal A 1 { } decalgroup G 2 { A 10 }", db, errors), string.Join("; ", errors));
        var group = Assert.Single(db.Groups);
        Assert.Equal("G", group.Name);
        Assert.Equal(10, group.Entries[0].Weight);
    }
}
