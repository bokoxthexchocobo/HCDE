namespace HCDE.Gamedata;

/// <summary>Native <c>DEFINE_PROPERTY(translation, L, Actor)</c> from <c>thingdef_properties.cpp</c>.</summary>
public static class DecorateTranslationPropertyHandler
{
    public const int MaxStandardTranslation = 6;

    public sealed class TranslationParseResult
    {
        public int TranslationId { get; set; }
        public bool UsesCustomRanges { get; set; }
        public List<string> CustomRanges { get; } = new();
    }

    public static bool TryParsePropertyTokens(
        IReadOnlyList<string> tokens,
        out List<DecoratePropertyValue> values,
        out string? error)
    {
        values = new List<DecoratePropertyValue>();
        error = null;
        if (tokens.Count == 0)
        {
            error = "translation requires parameters.";
            return false;
        }

        if (int.TryParse(tokens[0], out var type))
        {
            values.Add(DecoratePropertyValue.FromInt(type));
            if (type == 0)
            {
                if (tokens.Count < 2 || !int.TryParse(tokens[1], out var trans))
                {
                    error = "Standard translation requires an index.";
                    return false;
                }

                values.Add(DecoratePropertyValue.FromInt(trans));
                return true;
            }

            for (var i = 1; i < tokens.Count; i++)
                values.Add(DecoratePropertyValue.FromString(tokens[i]));
            return true;
        }

        if (!DecoratePropertyParamParser.TryParse("L", tokens, out var parsed, out error))
            return false;
        values.AddRange(parsed!.Values);
        return true;
    }

    public static bool TryApply(
        IReadOnlyList<DecoratePropertyValue> values,
        DecorateActorDefinition definition,
        out string? error)
    {
        error = null;
        if (values.Count == 0)
        {
            error = "translation requires at least one parameter.";
            return false;
        }

        var result = new TranslationParseResult();
        if (!TryParse(values, result, out error))
            return false;

        definition.TranslationId = result.TranslationId;
        definition.TranslationUsesCustomRanges = result.UsesCustomRanges;
        definition.TranslationCustomRanges.Clear();
        definition.TranslationCustomRanges.AddRange(result.CustomRanges);
        return true;
    }

    public static bool TryParse(IReadOnlyList<DecoratePropertyValue> values, TranslationParseResult result, out string? error)
    {
        error = null;
        var type = values[0].IntValue;
        if (type == 0)
        {
            if (values.Count < 2)
            {
                error = "Standard translation requires an index.";
                return false;
            }

            var trans = values[1].IntValue;
            if (trans < 0 || trans > MaxStandardTranslation)
            {
                error = $"Translation must be in the range [0,{MaxStandardTranslation}].";
                return false;
            }

            result.TranslationId = trans;
            return true;
        }

        result.UsesCustomRanges = true;
        for (var i = 1; i < values.Count; i++)
        {
            var token = values[i].StringValue;
            if (!string.IsNullOrEmpty(token))
                result.CustomRanges.Add(token);
        }

        result.TranslationId = -1;
        return true;
    }
}
