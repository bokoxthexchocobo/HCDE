namespace HCDE.Gamedata;

/// <summary>Native <c>DispatchScriptProperty</c> value decoding for scripted actor fields (no VM offsets).</summary>
public static class DecorateScriptPropertyDispatcher
{
    public enum ScriptFieldKind
    {
        Bool,
        Name,
        Sound,
        ColorInt,
        ColorRgb,
        Int,
        Float,
        String,
        ClassPointer,
    }

    public sealed class ScriptFieldBinding
    {
        public required string Name { get; init; }
        public ScriptFieldKind Kind { get; init; }
        public bool Meta { get; init; }
        public Action<DecorateScriptPropertyTarget, DecorateScriptPropertyValue>? Apply { get; init; }
    }

    public sealed class DecorateScriptPropertyTarget
    {
        public Dictionary<string, bool> MetaBools { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Strings { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> Ints { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, double> Floats { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string?> ClassPointers { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public readonly struct DecorateScriptPropertyValue
    {
        public bool BoolValue { get; init; }
        public string StringValue { get; init; }
        public int IntValue { get; init; }
        public double DoubleValue { get; init; }
    }

    public static bool TryDispatch(
        IReadOnlyList<ScriptFieldBinding> fields,
        IReadOnlyList<string> tokens,
        DecorateScriptPropertyTarget target,
        out string? error)
    {
        error = null;
        if (fields.Count == 0)
        {
            error = "Property has no fields.";
            return false;
        }

        var cursor = 0;
        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            if (i > 0)
            {
                if (cursor >= tokens.Count || tokens[cursor] != ",")
                {
                    error = $"Expected ',' before field '{field.Name}'.";
                    return false;
                }

                cursor++;
            }

            if (!TryReadValue(field.Kind, tokens, ref cursor, out var value, out error))
                return false;

            field.Apply?.Invoke(target, value);
            ApplyDefault(field, target, value);
        }

        return true;
    }

    private static void ApplyDefault(ScriptFieldBinding field, DecorateScriptPropertyTarget target, DecorateScriptPropertyValue value)
    {
        var key = field.Name;
        switch (field.Kind)
        {
            case ScriptFieldKind.Bool:
                target.MetaBools[key] = value.BoolValue;
                break;
            case ScriptFieldKind.Int:
            case ScriptFieldKind.ColorInt:
                target.Ints[key] = value.IntValue;
                break;
            case ScriptFieldKind.Float:
                target.Floats[key] = value.DoubleValue;
                break;
            case ScriptFieldKind.Name:
            case ScriptFieldKind.Sound:
            case ScriptFieldKind.String:
                target.Strings[key] = value.StringValue;
                break;
            case ScriptFieldKind.ClassPointer:
                target.ClassPointers[key] = string.IsNullOrEmpty(value.StringValue) ? null : value.StringValue;
                break;
        }
    }

    private static bool TryReadValue(
        ScriptFieldKind kind,
        IReadOnlyList<string> tokens,
        ref int cursor,
        out DecorateScriptPropertyValue value,
        out string? error)
    {
        error = null;
        value = default;
        switch (kind)
        {
            case ScriptFieldKind.Bool:
                if (cursor < tokens.Count && int.TryParse(tokens[cursor], out var num))
                {
                    cursor++;
                    value = new DecorateScriptPropertyValue { BoolValue = num != 0 };
                    return true;
                }

                value = new DecorateScriptPropertyValue { BoolValue = true };
                return true;
            case ScriptFieldKind.Int:
            case ScriptFieldKind.ColorInt:
                if (cursor >= tokens.Count || !int.TryParse(tokens[cursor], out var i))
                {
                    error = "Expected integer token.";
                    return false;
                }

                cursor++;
                value = new DecorateScriptPropertyValue { IntValue = i };
                return true;
            case ScriptFieldKind.Float:
                if (cursor >= tokens.Count || !double.TryParse(tokens[cursor], out var d))
                {
                    error = "Expected float token.";
                    return false;
                }

                cursor++;
                value = new DecorateScriptPropertyValue { DoubleValue = d };
                return true;
            default:
                if (cursor >= tokens.Count)
                {
                    error = "Expected string token.";
                    return false;
                }

                cursor++;
                value = new DecorateScriptPropertyValue { StringValue = tokens[cursor - 1] };
                return true;
        }
    }
}
