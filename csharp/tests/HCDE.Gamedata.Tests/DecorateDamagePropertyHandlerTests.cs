namespace HCDE.Gamedata.Tests;

public class DecorateDamagePropertyHandlerTests
{
    [Fact]
    public void IntegerDamage_SetsScalar()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecoratePropertyParamParser.TryParse("X", ["12"], out var parsed, out _));
        Assert.True(DecorateActorDamagePropertyHandler.TryApply(parsed!.Values, def, out _));
        Assert.Equal(12, def.Damage);
        Assert.False(def.DamageUsesExpression);
    }

    [Fact]
    public void ExpressionDamage_StoresTokens()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecoratePropertyParamParser.TryParse("X", ["(", "Random", "(", "1", ",", "8", ")", ")"], out var parsed, out _));
        Assert.True(DecorateActorDamagePropertyHandler.TryApply(parsed!.Values, def, out _));
        Assert.True(def.DamageUsesExpression);
        Assert.NotEmpty(def.DamageExpressionTokens);
    }

    [Fact]
    public void DamageTypeBlock_ParsesFactorAndNoArmor()
    {
        var tokens = new List<string> { "Fire", "{", "FACTOR", "0", "NOARMOR", "}" };
        var index = 0;
        var definition = new DecorateDamageTypeDefinitionParser.DamageTypeDefinition();
        Assert.True(DecorateDamageTypeDefinitionParser.TryParseBlock(tokens, ref index, definition, out _));
        Assert.Equal("Fire", definition.DamageType);
        Assert.True(definition.NoArmor);
        Assert.True(definition.ReplaceFactor);
    }
}
