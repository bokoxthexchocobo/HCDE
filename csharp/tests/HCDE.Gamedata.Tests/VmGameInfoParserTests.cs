namespace HCDE.Gamedata.Tests;

public class VmGameInfoParserTests
{
    [Fact]
    public void WeaponSlot_ParsesList()
    {
        var tokens = VmLockDefTokenize.Tokenize("0 , Fist , Pistol }");
        var i = 0;
        var doc = new VmGameInfoDocument();
        var errors = new List<string>();
        Assert.True(VmGameInfoParser.TryParseKey("weaponslot", tokens, ref i, doc, errors), string.Join("; ", errors));
        Assert.Equal("Pistol", doc.WeaponSlots[0][1]);
    }

    [Fact]
    public void CheckGame_TreatsChexAsDoom()
    {
        Assert.True(VmGameInfoParser.CheckGame("doom", "Chex"));
        Assert.False(VmGameInfoParser.CheckGame("heretic", "Doom"));
    }

    [Fact]
    public void GetFinalePage_WrapsBounds()
    {
        var pages = new List<string> { "FLAT1", "FLAT2" };
        Assert.Equal("FLAT1", VmGameInfoParser.GetFinalePage(pages, 99));
    }
}
