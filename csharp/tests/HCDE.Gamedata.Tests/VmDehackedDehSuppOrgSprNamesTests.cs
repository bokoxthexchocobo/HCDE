namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppOrgSprNamesTests
{
    [Fact]
    public void TryParseSpriteName_RequiresFourChars()
    {
        Assert.True(VmDehackedDehSuppOrgSprNames.TryParseSpriteName("PLAY", out var name));
        Assert.Equal("PLAY", name);
        Assert.False(VmDehackedDehSuppOrgSprNames.TryParseSpriteName("P", out _));
    }

    [Fact]
    public void TryParseOrgSprNamesSection_SetsOrgSprOrgSize()
    {
        Assert.True(VmDehackedDehSuppOrgSprNames.TryParseOrgSprNamesSection(
            ["{", "PLAY", ",", "TROO", "}"],
            [],
            out var names,
            out var orgSize,
            out _));
        Assert.Equal(2, names.Count);
        Assert.Equal(2, orgSize);
    }

    [Fact]
    public void TryParseOrgSprNamesSection_SkipsAppendWhenExisting()
    {
        var existing = new List<string> { "PLAY" };
        Assert.True(VmDehackedDehSuppOrgSprNames.TryParseOrgSprNamesSection(
            ["TROO"],
            existing,
            out var names,
            out var orgSize,
            out _));
        Assert.Single(names);
        Assert.Equal("PLAY", names[0]);
        Assert.Equal(1, orgSize);
    }

    [Fact]
    public void TryParseOrgSprNamesSection_RejectsInvalidSprite()
    {
        Assert.False(VmDehackedDehSuppOrgSprNames.TryParseOrgSprNamesSection(
            ["BAD"],
            [],
            out _,
            out _,
            out var error));
        Assert.Equal(VmDehackedDehSuppOrgSprNames.InvalidSpriteNameMessage, error);
    }
}
