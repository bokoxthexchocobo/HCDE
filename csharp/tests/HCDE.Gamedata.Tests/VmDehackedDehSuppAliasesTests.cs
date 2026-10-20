namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppAliasesTests
{
    [Fact]
    public void ParseAliasEntries_ReadsPairsInOrder()
    {
        var rows = new (string, string)[] { ("FireShotgun", "A_FireShotgun"), ("OpenDoor", "A_OpenDoor") };
        var list = VmDehackedDehSuppAliases.ParseAliasEntries(rows);
        Assert.Equal(2, list.Count);
        Assert.Equal("FireShotgun", list[0].Alias);
        Assert.Equal("A_OpenDoor", list[1].Name);
    }

    [Fact]
    public void TryParseAliasPair_RejectsBlank()
    {
        Assert.False(VmDehackedDehSuppAliases.TryParseAliasPair(" ", "A_Fire", out _));
    }
}
