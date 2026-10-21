namespace HCDE.Scripting;

/// <summary>Dot/cross, type check, dynamic cast, and ternary resolve from <c>codegen.cpp</c> (5142–5442).</summary>
public static class ZccConditionalAndTypeResolve
{
    public const int TkCross = 0x130;
    public const int TkDot = 0x131;
    public const int RegtFloat = 2;
    public const int RegtNil = 0;
    public const int TypeVector3 = 10;
    public const int TypeFloat64 = 11;
    public const int TypeBool = 12;
    public const int TypeSInt32 = 13;
    public const int TypeTranslationId = 14;
    public const int TypeNullPtr = 15;

    public static bool DotCrossOperandsCompatible(
        bool leftVector,
        bool sameValueType,
        int op,
        int leftValueType) =>
        leftVector && sameValueType && (op != TkCross || leftValueType == TypeVector3);

    public static int DotCrossResultType(int op) => op == TkCross ? TypeVector3 : TypeFloat64;

public static bool DynamicCastNeedsObjectPointer(bool isPointer, bool isConst) => isPointer && isConst;

    public static string? MergeConditionalValueType(
        bool sameType,
        bool bothBool,
        bool bothInteger,
        bool bothNumeric,
        bool leftPointerRightNull,
        bool rightPointerLeftNull,
        string? trueType,
        string? falseType)
    {
        if (sameType) return trueType;
        if (bothBool) return "bool";
        if (bothInteger) return "sint32";
        if (bothNumeric) return "float64";
        if (leftPointerRightNull) return trueType;
        if (rightPointerLeftNull) return falseType;
        return null;
    }

    public static string TranslationIdBranchValueType(int version, int threshold412) =>
        version < threshold412 ? "sint32" : "translationid";

    public static bool ConditionalIncompatible(bool trueNull, bool falseNull, bool resultNil) =>
        trueNull || falseNull || resultNil;

    public static bool NeedsConditionBoolCast(bool conditionIsBool) => !conditionIsBool;

public static bool ShouldFloatCastBranch(bool isFloat, int regType) => isFloat && regType != RegtFloat;
}
