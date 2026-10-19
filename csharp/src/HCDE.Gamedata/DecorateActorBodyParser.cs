namespace HCDE.Gamedata;

/// <summary>Native <c>ParseActor</c> body dispatch loop (properties, flags, states keyword) without VM compile.</summary>
public static class DecorateActorBodyParser
{
    public enum BodyTokenKind
    {
        Const,
        Enum,
        Var,
        Identifier,
        States,
        FlagModifier,
        Unknown,
    }

    public sealed class BodyParseContext
    {
        public DecorateActorDefinition Definition { get; } = new();
        public bool StateSet { get; set; }
        public bool DropItemSet { get; set; }
        public List<string> Diagnostics { get; } = new();
    }

    public static BodyTokenKind ClassifyToken(string token)
    {
        if (token.Equals("states", StringComparison.OrdinalIgnoreCase))
            return BodyTokenKind.States;
        return token switch
        {
            "const" => BodyTokenKind.Const,
            "enum" => BodyTokenKind.Enum,
            "var" => BodyTokenKind.Var,
            "+" or "-" => BodyTokenKind.FlagModifier,
            _ => BodyTokenKind.Identifier,
        };
    }

    public static bool TryParseBody(IReadOnlyList<string> tokens, BodyParseContext context, out string? error)
    {
        error = null;
        if (tokens.Count == 0 || tokens[0] != "{")
        {
            error = "Actor body must begin with '{'.";
            return false;
        }

        var index = 1;
        while (index < tokens.Count)
        {
            if (tokens[index] == "}")
                break;
            if (tokens[index] == ";")
            {
                index++;
                continue;
            }

            var kind = ClassifyToken(tokens[index]);
            switch (kind)
            {
                case BodyTokenKind.States:
                    context.StateSet = true;
                    index++;
                    if (index < tokens.Count && tokens[index] == "}")
                        break;
                    continue;
                case BodyTokenKind.FlagModifier:
                {
                    var mod = tokens[index++];
                    if (index >= tokens.Count)
                    {
                        error = "Flag modifier requires a flag name.";
                        return false;
                    }

                    var flag = tokens[index++];
                    if (!DecorateActorFlagModifier.TryModify(context.Definition.Flags, flag, mod == "+", out var flagError))
                        context.Diagnostics.Add(flagError ?? $"Unknown flag {flag}");
                    if (index < tokens.Count && tokens[index] == ";")
                        index++;
                    break;
                }
                case BodyTokenKind.Identifier:
                {
                    var property = tokens[index++];
                    var propertyTokens = ReadPropertyTokens(tokens, ref index);
                    if (!DecorateActorPropertyRouter.TryApplyProperty(property, context.Definition.ActorClass, propertyTokens, context.Definition, out var propError))
                    {
                        context.Diagnostics.Add(propError ?? $"Failed property {property}");
                    }

                    if (string.Equals(property, "dropitem", StringComparison.OrdinalIgnoreCase))
                        context.DropItemSet = true;
                    break;
                }
                case BodyTokenKind.Const:
                case BodyTokenKind.Enum:
                case BodyTokenKind.Var:
                    index++;
                    SkipStatement(tokens, ref index);
                    break;
                default:
                    error = $"Unexpected token '{tokens[index]}' in actor body.";
                    return false;
            }
        }

        if (index < tokens.Count && tokens[index] == "}")
            index++;

        return true;
    }

    private static List<string> ReadPropertyTokens(IReadOnlyList<string> tokens, ref int index)
    {
        var list = new List<string>();
        while (index < tokens.Count && tokens[index] != ";")
            list.Add(tokens[index++]);
        if (index < tokens.Count && tokens[index] == ";")
            index++;
        return list;
    }

    private static void SkipStatement(IReadOnlyList<string> tokens, ref int index)
    {
        while (index < tokens.Count && tokens[index] != ";")
            index++;
        if (index < tokens.Count)
            index++;
    }
}
