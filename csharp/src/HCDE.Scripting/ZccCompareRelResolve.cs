namespace HCDE.Scripting;

/// <summary><c>FxCompareRel::Resolve</c> coercion prologue from <c>codegen.cpp</c> (3876–3933).</summary>
public static class ZccCompareRelResolve
{
    public enum CoerceAction
    {
        None,
        ReplaceRightViaStringConstToChar,
        ReplaceLeftViaStringConstToChar,
        CastRightTranslationIdToSint32,
        CastLeftTranslationIdToSint32,
        ResolveLeftStringCast,
        ResolveRightStringCast,
        ResultTypeString,
    }

    public static bool NeedsTranslationCastToInt(bool numericSide, bool translationIdSide, int version) =>
        numericSide && translationIdSide && version < ZccTypeCastResolve.MakeVersion(4, 12, 0);

    public static bool StringComparisonPath(bool leftString, bool rightString) => leftString || rightString;

    public static bool ReplaceRightViaStringConstToChar(bool leftNumeric, bool rightString, bool rightIsStringConstant) =>
        leftNumeric && rightString && rightIsStringConstant;

    public static bool ReplaceLeftViaStringConstToChar(bool rightNumeric, bool leftString, bool leftIsStringConstant) =>
        rightNumeric && leftString && leftIsStringConstant;

    public static CoerceAction? FirstNumericStringCoerce(
        bool leftNumeric,
        bool rightNumeric,
        bool leftString,
        bool rightString,
        bool leftStringConstToChar,
        bool rightStringConstToChar)
    {
        if (ReplaceRightViaStringConstToChar(leftNumeric, rightString, rightStringConstToChar))
            return CoerceAction.ReplaceRightViaStringConstToChar;
        if (ReplaceLeftViaStringConstToChar(rightNumeric, leftString, leftStringConstToChar))
            return CoerceAction.ReplaceLeftViaStringConstToChar;
        return null;
    }

    public static CoerceAction? TranslationIdCoerce(
        bool leftNumeric,
        bool rightNumeric,
        bool leftTranslationId,
        bool rightTranslationId,
        int version)
    {
        if (NeedsTranslationCastToInt(leftNumeric, rightTranslationId, version))
            return CoerceAction.CastRightTranslationIdToSint32;
        if (NeedsTranslationCastToInt(rightNumeric, leftTranslationId, version))
            return CoerceAction.CastLeftTranslationIdToSint32;
        return null;
    }

    public static bool NeedsLeftStringCast(bool leftIsString) => !leftIsString;

    public static bool NeedsRightStringCast(bool rightIsString) => !rightIsString;

    public static IReadOnlyList<CoerceAction> PlanStringPathCoercions(bool leftString, bool rightString)
    {
        if (!StringComparisonPath(leftString, rightString))
            return Array.Empty<CoerceAction>();
        var steps = new List<CoerceAction>(3);
        if (NeedsLeftStringCast(leftString)) steps.Add(CoerceAction.ResolveLeftStringCast);
        if (NeedsRightStringCast(rightString)) steps.Add(CoerceAction.ResolveRightStringCast);
        steps.Add(CoerceAction.ResultTypeString);
        return steps;
    }

    public static bool CanPromoteNumericCompare(bool leftNumeric, bool rightNumeric) =>
        leftNumeric && rightNumeric;

    public static bool EntersNumericCompareBody(bool leftString, bool rightString, bool leftNumeric, bool rightNumeric) =>
        !StringComparisonPath(leftString, rightString) && leftNumeric && rightNumeric;
}
