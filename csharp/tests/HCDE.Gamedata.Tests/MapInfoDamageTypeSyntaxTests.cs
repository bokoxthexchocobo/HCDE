namespace HCDE.Gamedata.Tests;

public class MapInfoDamageTypeSyntaxTests
{
    [Fact]
    public void QuotedNamesAndObituariesAreStoredWithoutDelimiters()
    {
        Assert.True(MapInfoParser.TryParse("DamageType \"Acid\" { Factor = 0.5 Obituary = \"acid death\" }", out var set, out var error), error);
        var definition = Assert.Single(set.DamageTypes);
        Assert.Equal("Acid", definition.Name); Assert.Equal("acid death", definition.Obituary);
    }

    [Fact]
    public void OldStyleMapEndsBeforeDamageTypeDeclaration()
    {
        Assert.True(MapInfoParser.TryParse("map MAP01 \"Test\" levelnum 1 DamageType Acid { NoArmor }", out var set, out var error), error);
        Assert.Equal(1, Assert.Single(set.Maps).LevelNum);
        Assert.True(Assert.Single(set.DamageTypes).NoArmor);
    }

    [Theory]
    [InlineData("DamageType \"\" { }")]
    [InlineData("DamageType \"Acid")]
    [InlineData("DamageType Acid { Obituary = \"broken }")]
    public void InvalidQuotedValuesFail(string text)
    {
        Assert.False(MapInfoParser.TryParse(text, out _, out var error));
        Assert.NotNull(error);
    }
}
