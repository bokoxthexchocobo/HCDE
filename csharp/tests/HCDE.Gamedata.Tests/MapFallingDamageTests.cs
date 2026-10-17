namespace HCDE.Gamedata.Tests;

public class MapFallingDamageTests
{
    [Theory]
    [InlineData("", true, true)]
    [InlineData("nomonsterfallingdamage", false, true)]
    [InlineData("nomonsterfallingdamage monsterfallingdamage", true, true)]
    public void MonsterFlagsOverrideInheritedEnablementIndependently(string fields, bool enabled, bool proper)
    {
        Assert.True(MapInfoParser.TryParse("defaultmap { monsterfallingdamage propermonsterfallingdamage } map MAP01 { " + fields + " }", out var data, out var error), error);
        Assert.Equal(enabled, data.FindMap("MAP01")!.MonsterFallingDamage);
        Assert.Equal(proper, data.FindMap("MAP01")!.ProperMonsterFallingDamage);
    }

    [Theory]
    [InlineData("fallingdamage", FallingDamageStyle.Hexen)]
    [InlineData("oldfallingdamage", FallingDamageStyle.ZDoom)]
    [InlineData("forcefallingdamage", FallingDamageStyle.ZDoom)]
    [InlineData("strifefallingdamage", FallingDamageStyle.Strife)]
    [InlineData("nofallingdamage", FallingDamageStyle.None)]
    [InlineData("strifefallingdamage fallingdamage", FallingDamageStyle.Hexen)]
    public void NativeFlagsOverrideInheritedStyle(string fields, FallingDamageStyle expected)
    {
        Assert.True(MapInfoParser.TryParse("defaultmap { fallingdamage } map MAP01 { " + fields + " }", out var data, out var error), error);
        Assert.Equal(expected, data.FindMap("MAP01")!.FallingDamage);
    }
}
