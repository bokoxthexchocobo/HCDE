namespace HCDE.Gamedata;

/// <summary>Native <c>ParseDamageDefinition</c> from <c>thingdef_parse.cpp</c>.</summary>
public static class DecorateDamageTypeDefinitionParser
{
    public sealed class DamageTypeDefinition
    {
        public string DamageType { get; set; } = "";
        public double DefaultFactor { get; set; } = 1;
        public bool ReplaceFactor { get; set; }
        public bool NoArmor { get; set; }
    }

    public static bool TryParseBlock(IReadOnlyList<string> tokens, ref int index, DamageTypeDefinition definition, out string? error)
    {
        error = null;
        if (index >= tokens.Count)
        {
            error = "Expected damage type name.";
            return false;
        }

        definition.DamageType = tokens[index++];
        if (index >= tokens.Count || tokens[index] != "{")
        {
            error = "Expected '{' after damage type name.";
            return false;
        }

        index++;
        while (index < tokens.Count && tokens[index] != "}")
        {
            var keyword = tokens[index++];
            switch (keyword.ToUpperInvariant())
            {
                case "FACTOR":
                    if (index >= tokens.Count || !double.TryParse(tokens[index++], out var factor))
                    {
                        error = "FACTOR requires a float value.";
                        return false;
                    }

                    definition.DefaultFactor = factor;
                    if (factor == 0)
                        definition.ReplaceFactor = true;
                    break;
                case "REPLACEFACTOR":
                    definition.ReplaceFactor = true;
                    break;
                case "NOARMOR":
                    definition.NoArmor = true;
                    break;
                default:
                    error = $"Unexpected data ({keyword}) in damagetype definition.";
                    return false;
            }
        }

        if (index >= tokens.Count || tokens[index] != "}")
        {
            error = "Unclosed damage type block.";
            return false;
        }

        index++;
        return true;
    }
}
