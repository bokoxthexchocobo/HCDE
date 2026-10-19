using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>
/// Subset of native <c>ParsePropertyParams</c> from <c>thingdef_parse.cpp</c> (no expression compiler).</summary>
public static class DecoratePropertyParamParser
{
    public sealed class ParseResult
    {
        public required List<DecoratePropertyValue> Values { get; init; }
        public required string ParamSpec { get; init; }
    }

    public static bool TryParse(string paramSpec, IReadOnlyList<string> tokens, out ParseResult? result, out string? error)
    {
        result = null;
        error = null;
        if (string.IsNullOrEmpty(paramSpec) || paramSpec[0] == '0')
        {
            result = new ParseResult { ParamSpec = paramSpec, Values = [] };
            return true;
        }

        var values = new List<DecoratePropertyValue>();
        var index = 0;
        for (var pi = 0; pi < paramSpec.Length; pi++)
        {
            var letter = char.ToUpperInvariant(paramSpec[pi]);
            var optionalComma = pi + 1 < paramSpec.Length && paramSpec[pi + 1] == '_';
            if (optionalComma)
                pi++;

            if (index >= tokens.Count)
            {
                error = $"Expected token for parameter '{letter}' at index {index}.";
                return false;
            }

            switch (letter)
            {
                case 'I':
                    if (!TryReadInt(tokens, ref index, out var i))
                    {
                        error = "Expected integer.";
                        return false;
                    }
                    values.Add(DecoratePropertyValue.FromInt(i));
                    break;
                case 'F':
                    if (!TryReadDouble(tokens, ref index, out var d))
                    {
                        error = "Expected float.";
                        return false;
                    }
                    values.Add(DecoratePropertyValue.FromDouble(d));
                    break;
                case 'S':
                case 'T':
                    values.Add(DecoratePropertyValue.FromString(ReadStringToken(tokens, ref index, letter == 'T')));
                    break;
                case 'Z':
                    if (TryReadDouble(tokens, ref index, out _))
                    {
                        index--;
                        break;
                    }
                    values.Add(DecoratePropertyValue.FromString(ReadStringToken(tokens, ref index, decodeEscapes: true)));
                    break;
                case 'C':
                    if (TryReadInt(tokens, ref index, out var r) && index < tokens.Count && tokens[index] == ",")
                    {
                        index++;
                        if (!TryReadInt(tokens, ref index, out var g) || index >= tokens.Count || tokens[index] != ",")
                        {
                            error = "Color tuple requires R,G,B integers.";
                            return false;
                        }
                        index++;
                        if (!TryReadInt(tokens, ref index, out var b))
                        {
                            error = "Color tuple requires R,G,B integers.";
                            return false;
                        }
                        values.Add(DecoratePropertyValue.FromColorRgb(
                            Math.Clamp(r, 0, 255),
                            Math.Clamp(g, 0, 255),
                            Math.Clamp(b, 0, 255)));
                    }
                    else
                    {
                        index = Math.Max(0, index - 1);
                        values.Add(DecoratePropertyValue.FromString(ReadStringToken(tokens, ref index, decodeEscapes: false)));
                    }
                    break;
                case 'M':
                    if (!DecorateFlagExpression.TryParse(tokens[index], DecorateFlagExpression.MorphStyleFlags, out var morph))
                    {
                        error = $"Unknown morph style '{tokens[index]}'.";
                        return false;
                    }
                    index++;
                    values.Add(DecoratePropertyValue.FromInt(morph));
                    break;
                case 'N':
                    if (!DecorateFlagExpression.TryParse(tokens[index], DecorateFlagExpression.ThingActivationFlags, out var activation))
                    {
                        error = $"Unknown thing activation '{tokens[index]}'.";
                        return false;
                    }
                    index++;
                    values.Add(DecoratePropertyValue.FromInt(activation));
                    break;
                case 'L':
                    if (TryReadInt(tokens, ref index, out var listMode))
                    {
                        values.Add(DecoratePropertyValue.FromInt(listMode));
                    }
                    else
                    {
                        index = Math.Max(0, index - 1);
                        var strings = new List<string>();
                        while (index < tokens.Count)
                        {
                            strings.Add(ReadStringToken(tokens, ref index, decodeEscapes: false));
                            if (index < tokens.Count && tokens[index] == ",")
                            {
                                index++;
                                continue;
                            }
                            break;
                        }
                        values.Add(DecoratePropertyValue.FromStringList(strings));
                    }
                    break;
                case 'X':
                    if (tokens[index] == "(")
                    {
                        index++;
                        var exprTokens = new List<string>();
                        while (index < tokens.Count && tokens[index] != ")")
                            exprTokens.Add(tokens[index++]);
                        if (index >= tokens.Count || tokens[index] != ")")
                        {
                            error = "Unclosed expression.";
                            return false;
                        }
                        index++;
                        values.Add(DecoratePropertyValue.FromExpression(exprTokens));
                    }
                    else if (TryReadInt(tokens, ref index, out var x))
                    {
                        values.Add(DecoratePropertyValue.FromInt(x));
                    }
                    else
                    {
                        error = "Expression parameter requires '(' or integer.";
                        return false;
                    }
                    break;
                default:
                    error = $"Unsupported parameter type '{letter}'.";
                    return false;
            }

            if (pi + 1 >= paramSpec.Length || (optionalComma && pi + 1 < paramSpec.Length && paramSpec[pi + 1] == '_'))
                continue;
            if (index < tokens.Count && tokens[index] == ",")
                index++;
        }

        result = new ParseResult { ParamSpec = paramSpec, Values = values };
        return true;
    }

    private static bool TryReadInt(IReadOnlyList<string> tokens, ref int index, out int value)
    {
        value = 0;
        if (index >= tokens.Count)
            return false;
        if (!int.TryParse(tokens[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return false;
        index++;
        return true;
    }

    private static bool TryReadDouble(IReadOnlyList<string> tokens, ref int index, out double value)
    {
        value = 0;
        if (index >= tokens.Count)
            return false;
        if (!double.TryParse(tokens[index], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return false;
        index++;
        return true;
    }

    private static string ReadStringToken(IReadOnlyList<string> tokens, ref int index, bool decodeEscapes)
    {
        var raw = tokens[index++];
        if (!decodeEscapes)
            return raw;
        return raw.Replace("\\n", "\n", StringComparison.Ordinal).Replace("\\t", "\t", StringComparison.Ordinal);
    }
}

public readonly struct DecoratePropertyValue
{
    public DecoratePropertyKind Kind { get; init; }
    public int IntValue { get; init; }
    public double DoubleValue { get; init; }
    public string? StringValue { get; init; }
    public IReadOnlyList<string>? StringList { get; init; }
    public IReadOnlyList<string>? ExpressionTokens { get; init; }
    public int ColorR { get; init; }
    public int ColorG { get; init; }
    public int ColorB { get; init; }

    public static DecoratePropertyValue FromInt(int value) =>
        new() { Kind = DecoratePropertyKind.Integer, IntValue = value };

    public static DecoratePropertyValue FromDouble(double value) =>
        new() { Kind = DecoratePropertyKind.Float, DoubleValue = value };

    public static DecoratePropertyValue FromString(string value) =>
        new() { Kind = DecoratePropertyKind.String, StringValue = value };

    public static DecoratePropertyValue FromStringList(IReadOnlyList<string> values) =>
        new() { Kind = DecoratePropertyKind.StringList, StringList = values };

    public static DecoratePropertyValue FromExpression(IReadOnlyList<string> tokens) =>
        new() { Kind = DecoratePropertyKind.Expression, ExpressionTokens = tokens };

    public static DecoratePropertyValue FromColorRgb(int r, int g, int b) =>
        new() { Kind = DecoratePropertyKind.ColorRgb, ColorR = r, ColorG = g, ColorB = b, IntValue = (r << 16) | (g << 8) | b };
}

public enum DecoratePropertyKind
{
    Integer,
    Float,
    String,
    StringList,
    Expression,
    ColorRgb,
}
