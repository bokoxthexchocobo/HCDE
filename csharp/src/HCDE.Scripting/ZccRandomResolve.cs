namespace HCDE.Scripting;

/// <summary>Random expression resolve from <c>codegen.cpp</c> (6203–6591).</summary>
public static class ZccRandomResolve
{
    public enum RandomRangeOutcome
    {
        Ok,
        MissingOperand,
        TypeMismatch,
    }

    public static RandomRangeOutcome ValidateRandomRange(bool hasMin, bool hasMax, bool minOk, bool maxOk, bool typesMatch)
    {
        if (!hasMin || !hasMax) return RandomRangeOutcome.MissingOperand;
        if (!minOk || !maxOk) return RandomRangeOutcome.MissingOperand;
        return typesMatch ? RandomRangeOutcome.Ok : RandomRangeOutcome.TypeMismatch;
    }

    public static bool RandomPickAllResolved(int count, int resolved) => count > 0 && resolved == count;

    public static bool RandomPickTypeUniform(IEnumerable<bool> matches) => matches.All(x => x);

    public static bool Random2MaskPresent(bool hasMask) => hasMask;

    public static bool RandomSeedOperandResolved(bool seedResolved) => seedResolved;

    public static int PickIndexForEmit(int choiceCount) => choiceCount - 1;

    public static int RandomPickSwitchUpperBound(int choiceCount) => choiceCount - 1;
}
