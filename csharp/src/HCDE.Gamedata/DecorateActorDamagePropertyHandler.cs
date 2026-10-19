namespace HCDE.Gamedata;

/// <summary>Native <c>DEFINE_PROPERTY(damage, X, Actor)</c> integer vs parenthesized expression.</summary>
public static class DecorateActorDamagePropertyHandler
{
    public static bool TryApply(IReadOnlyList<DecoratePropertyValue> values, DecorateActorDefinition definition, out string? error)
    {
        error = null;
        if (values.Count == 0)
        {
            error = "damage requires a value.";
            return false;
        }

        var first = values[0];
        if (first.Kind == DecoratePropertyKind.Expression)
        {
            definition.DamageUsesExpression = true;
            definition.DamageExpressionTokens = first.ExpressionTokens?.ToArray() ?? Array.Empty<string>();
            definition.Damage = 0;
            return true;
        }

        definition.DamageUsesExpression = false;
        definition.Damage = first.IntValue;
        return true;
    }
}
