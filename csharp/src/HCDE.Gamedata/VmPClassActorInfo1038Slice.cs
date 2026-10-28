namespace HCDE.Gamedata;

/// <summary>
/// <c>GetReplacement</c> / <c>GetReplacee</c> chains and damage/pain setters from <c>info.cpp</c> (**650–799**).
/// Chunk **1038**; follows **1037** (**500–649**).
/// </summary>
public static class VmPClassActorInfo1038Slice
{
    public readonly record struct ReplacementResolveResult(
        string ResolvedType,
        bool ReturnSelf,
        bool EventHandlerFinal);

    public readonly record struct SkillReplacedByFailure(
        bool Failed,
        bool ClearLookSkill,
        string ClearedReplacedBy,
        string ClearedReplacement);

    public static void ClearSkillReplacementNamesAfterFailure(ref bool lookSkill, ref string skillReplaceName)
    {
        lookSkill = false;
        skillReplaceName = "";
    }

    public static bool ShouldSkipReplacementWhenLevelNull(bool hasLevel) => !hasLevel;

    public static ReplacementResolveResult ResolveEventHandlerReplacement(
        bool eventHandlerMatched,
        string? handlerReplacementType,
        string selfType,
        bool replaceeClientSide,
        bool replacementClientSide)
    {
        if (!eventHandlerMatched)
            return new ReplacementResolveResult(selfType, ReturnSelf: false, EventHandlerFinal: false);

        if (handlerReplacementType == null)
            return new ReplacementResolveResult(selfType, ReturnSelf: true, EventHandlerFinal: true);

        var ok = VmPClassActorInfo1037Slice.VerifyClientSideReplacement(
            replaceeClientSide,
            replacementClientSide);
        return new ReplacementResolveResult(
            ok ? handlerReplacementType : selfType,
            ReturnSelf: !ok,
            EventHandlerFinal: true);
    }

    public static bool ShouldReturnSelfWithoutReplacement(
        bool decorateReplacementMissing,
        bool lookSkill,
        string skillReplaceName) =>
        decorateReplacementMissing && (!lookSkill || skillReplaceName.Length == 0);

    public static string ResolveSkillThenDecorateReplacement(
        bool lookSkill,
        string skillReplaceName,
        string decorateReplacementType,
        Func<string, bool, string> resolveRecursiveReplacement)
    {
        var repType = decorateReplacementType;
        if (lookSkill && skillReplaceName.Length > 0)
            repType = skillReplaceName;
        return resolveRecursiveReplacement(repType, lookSkill: false);
    }

    public static string FinalizeReplacementClientSide(
        string selfType,
        string candidateType,
        bool selfClientSide,
        bool candidateClientSide)
    {
        if (VmPClassActorInfo1037Slice.VerifyClientSideReplacement(selfClientSide, candidateClientSide))
            return candidateType;
        return selfType;
    }

    public static SkillReplacedByFailure EvaluateSkillReplacedBy(
        bool lookSkill,
        int skillCount,
        int currentSkill,
        string typeName,
        string replacedByName,
        bool replacementClassExists,
        bool clientSideMatch)
    {
        if (!lookSkill || skillCount <= currentSkill || replacedByName.Length == 0)
            return new SkillReplacedByFailure(false, lookSkill, replacedByName, typeName);

        if (!replacementClassExists || !clientSideMatch)
            return new SkillReplacedByFailure(true, false, replacedByName, typeName);

        return new SkillReplacedByFailure(false, lookSkill, replacedByName, typeName);
    }

    public static SkillReplacedByFailure ApplySkillReplacedByFailureClear(SkillReplacedByFailure failure) =>
        failure.Failed
            ? failure with { ClearLookSkill = true }
            : failure;

    public static ReplacementResolveResult ResolveEventHandlerReplacee(
        bool eventHandlerMatched,
        string? handlerReplaceeType,
        string selfType,
        bool handlerClientSide,
        bool selfClientSide)
    {
        if (!eventHandlerMatched)
            return new ReplacementResolveResult(selfType, ReturnSelf: false, EventHandlerFinal: false);

        if (handlerReplaceeType == null)
            return new ReplacementResolveResult(selfType, ReturnSelf: true, EventHandlerFinal: true);

        var ok = VmPClassActorInfo1037Slice.VerifyClientSideReplacement(handlerClientSide, selfClientSide);
        return new ReplacementResolveResult(
            ok ? handlerReplaceeType : selfType,
            ReturnSelf: !ok,
            EventHandlerFinal: true);
    }

    public static bool ShouldReturnSelfWithoutReplacee(
        bool savedReplaceeMissing,
        bool lookSkill,
        string skillReplacedByName) =>
        savedReplaceeMissing && (!lookSkill || skillReplacedByName.Length == 0);

    public static string ResolveReplaceeChain(
        bool lookSkill,
        string skillReplacedByName,
        string savedReplaceeType,
        bool skillClassExists,
        Func<string, bool, string> resolveRecursiveReplacee)
    {
        var repType = savedReplaceeType;
        if (lookSkill && skillReplacedByName.Length > 0 && skillClassExists)
            repType = skillReplacedByName;
        return resolveRecursiveReplacee(repType, lookSkill: false);
    }

    public static bool TryUpdateDamageFactor(IList<(string Type, double Factor)> factors, string type, double factor)
    {
        for (var i = 0; i < factors.Count; i++)
        {
            if (!factors[i].Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                continue;
            factors[i] = (type, factor);
            return true;
        }

        return false;
    }

    public static void AddDamageFactor(IList<(string Type, double Factor)> factors, string type, double factor) =>
        factors.Add((type, factor));

    public static bool TryUpdatePainChance(IList<(string Type, int Chance)> chances, string type, int chance)
    {
        for (var i = 0; i < chances.Count; i++)
        {
            if (!chances[i].Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                continue;
            chances[i] = (type, ClampPainChance(chance));
            return true;
        }

        return false;
    }

    public static int ClampPainChance(int chance) => chance >= 0 ? Math.Min(chance, 256) : chance;

    public static bool ShouldPushPainChance(int chance) => chance >= 0;
}
