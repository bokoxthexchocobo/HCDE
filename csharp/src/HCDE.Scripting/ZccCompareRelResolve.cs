namespace HCDE.Scripting;

/// <summary><c>FxCompareRel::Resolve</c> from <c>codegen.cpp</c> (3876–4046).</summary>
public static class ZccCompareRelResolve
{
    public const int TkGeq = 0x100;
    public const int TkLeq = 0x101;

    public enum SignedUnsignedWarn
    {
        None,
        SignedVsOutOfRangeUnsignedConstant,
        UnsignedVsNegativeConstant,
        SignedUnsignedMix,
    }

    public static bool NeedsTranslationCastToInt(bool numericSide, bool translationIdSide, int version) =>
        numericSide && translationIdSide && version < ZccTypeCastResolve.MakeVersion(4, 12, 0);

    public static bool StringComparisonPath(bool leftString, bool rightString) => leftString || rightString;

    public static bool BothNumeric(bool leftNumeric, bool rightNumeric) => leftNumeric && rightNumeric;

    public static SignedUnsignedWarn UIntLeftSintRightWarning(
        bool versionOk,
        bool bothInteger,
        bool leftUInt,
        bool rightSint,
        bool leftConst,
        bool rightConst,
        uint leftUIntVal,
        int rightSintVal)
    {
        if (!versionOk || !bothInteger || !leftUInt || !rightSint) return SignedUnsignedWarn.None;
        if (leftConst && !rightConst && leftUIntVal > int.MaxValue)
            return SignedUnsignedWarn.SignedVsOutOfRangeUnsignedConstant;
        if (rightConst && !leftConst && rightSintVal < 0)
            return SignedUnsignedWarn.UnsignedVsNegativeConstant;
        if (!leftConst && !rightConst) return SignedUnsignedWarn.SignedUnsignedMix;
        return SignedUnsignedWarn.None;
    }

    public static SignedUnsignedWarn SintLeftUIntRightWarning(
        bool versionOk,
        bool bothInteger,
        bool leftSint,
        bool rightUInt,
        bool leftConst,
        bool rightConst,
        int leftSintVal,
        uint rightUIntVal)
    {
        if (!versionOk || !bothInteger || !leftSint || !rightUInt) return SignedUnsignedWarn.None;
        if (leftConst && !rightConst && leftSintVal < 0)
            return SignedUnsignedWarn.UnsignedVsNegativeConstant;
        if (rightConst && !leftConst && rightUIntVal > int.MaxValue)
            return SignedUnsignedWarn.SignedVsOutOfRangeUnsignedConstant;
        if (!leftConst && !rightConst) return SignedUnsignedWarn.SignedUnsignedMix;
        return SignedUnsignedWarn.None;
    }

    public static int FoldStringCompare(int op, int stringCompareResult) =>
        op switch
        {
            '<' => stringCompareResult < 0 ? 1 : 0,
            '>' => stringCompareResult > 0 ? 1 : 0,
            _ when op == TkGeq => stringCompareResult >= 0 ? 1 : 0,
            _ when op == TkLeq => stringCompareResult <= 0 ? 1 : 0,
            _ => 0,
        };

    public static int FoldFloatCompare(int op, double v1, double v2) =>
        op switch
        {
            '<' => v1 < v2 ? 1 : 0,
            '>' => v1 > v2 ? 1 : 0,
            _ when op == TkGeq => v1 >= v2 ? 1 : 0,
            _ when op == TkLeq => v1 <= v2 ? 1 : 0,
            _ => 0,
        };

    public static int FoldUIntCompare(int op, uint v1, uint v2) =>
        op switch
        {
            '<' => v1 < v2 ? 1 : 0,
            '>' => v1 > v2 ? 1 : 0,
            _ when op == TkGeq => v1 >= v2 ? 1 : 0,
            _ when op == TkLeq => v1 <= v2 ? 1 : 0,
            _ => 0,
        };

    public static int FoldIntCompare(int op, int v1, int v2) =>
        op switch
        {
            '<' => v1 < v2 ? 1 : 0,
            '>' => v1 > v2 ? 1 : 0,
            _ when op == TkGeq => v1 >= v2 ? 1 : 0,
            _ when op == TkLeq => v1 <= v2 ? 1 : 0,
            _ => 0,
        };

    public static int? TryFoldConstantCompare(
        bool bothConstant,
        bool resultIsString,
        bool resultIsFloat,
        bool resultIsUInt32,
        int op,
        int stringCompareResult,
        double f1,
        double f2,
        uint u1,
        uint u2,
        int i1,
        int i2)
    {
        if (!bothConstant) return null;
        if (resultIsString) return FoldStringCompare(op, stringCompareResult);
        if (resultIsFloat) return FoldFloatCompare(op, f1, f2);
        if (resultIsUInt32) return FoldUIntCompare(op, u1, u2);
        return FoldIntCompare(op, i1, i2);
    }
}
