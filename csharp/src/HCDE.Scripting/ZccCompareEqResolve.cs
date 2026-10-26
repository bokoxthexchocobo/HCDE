namespace HCDE.Scripting;

/// <summary><c>FxCompareEq::Resolve</c> from <c>codegen.cpp</c> (4175–4391).</summary>
public static class ZccCompareEqResolve
{
    public const int TkEq = '=';
    public const int TkNeq = '!';
    public const int TkApproxEq = '~';

    public static bool IdenticalValueTypes(string? left, string? right) =>
        string.Equals(left, right, StringComparison.Ordinal);

    public static bool BothNumeric(bool left, bool right) => left && right;

    public static bool IsApproxEqOperator(int op) => op == TkApproxEq;

    public static bool ApproxEqAllowedForRegType(int regTypeFloat, int regTypeString, int actual) =>
        actual == regTypeFloat || actual == regTypeString;

    public static bool BothConstant(bool left, bool right) => left && right;

    public static bool FoldStringEqual(bool approx, string left, string right)
    {
        if (approx) return left.Equals(right, StringComparison.OrdinalIgnoreCase);
        return !left.Equals(right, StringComparison.Ordinal);
    }

    public static bool FoldFloatEqual(int op, double left, double right) =>
        op == TkEq ? left == right : op == TkNeq ? left != right : Math.Abs(left - right) < 1e-6;

    public static bool FoldIntEqual(int op, int left, int right) =>
        op == TkEq ? left == right : left != right;

    public static bool LeftIntIsNull(int regTypeInt, int value) => value == 0;

    public static bool LeftFloatIsNull(double value) => value == 0;

    public static bool LeftPointerIsNull(IntPtr ptr) => ptr == IntPtr.Zero;

    public static bool ShouldUnaryNotRight(int op, bool leftIsNull) => op == TkEq && leftIsNull;

    public static bool ShouldBoolCastRight(int op, bool leftIsNull) => op != TkEq && leftIsNull;

    public static bool PointerTypesCompatible(bool same, bool rightNull, bool leftNull) =>
        same || rightNull || leftNull;

    public static bool StateLabelNullCompare(bool leftState, bool rightNull) =>
        leftState && rightNull;

    public static bool ValueTypeNil(int regTypeNil, int actual) => actual == regTypeNil;

    public static bool StringConstToCharEligible(bool leftNumeric, bool rightString) =>
        leftNumeric && rightString;

    public static bool TranslationIdLegacyCast(bool leftNumeric, bool rightTranslation, int version, int threshold) =>
        leftNumeric && rightTranslation && version < threshold;

    public static bool CanImplicitCastStringToName(bool leftString, bool rightNameLike) =>
        leftString && rightNameLike;

    public static bool RequestAddressForPointerCompare(bool pointerLeft, bool typesMatch) =>
        pointerLeft && typesMatch;

    public static bool RightNumericLeftString(bool rightNumeric, bool leftString) =>
        rightNumeric && leftString;

    public static bool VectorTypesComparable(bool leftV2, bool rightV2) => leftV2 && rightV2;

    public static bool CanCompareStateLabelToNull(bool leftState, bool rightNullPtr) =>
        leftState && rightNullPtr;

    public static bool CanCompareNullToStateLabel(bool rightState, bool leftNullPtr) =>
        rightState && leftNullPtr;

    public static bool PointerRegTypesMatch(int leftReg, int rightReg, int pointerReg) =>
        leftReg == pointerReg && rightReg == pointerReg;

    public static bool CompatiblePointersOrNull(bool compatible, bool leftNull, bool rightNull) =>
        compatible || leftNull || rightNull;

    public static bool ShouldPromoteAfterNumeric(bool leftNumeric, bool rightNumeric) =>
        leftNumeric && rightNumeric;

    public static bool SimplifyEqAgainstZero(bool isEq, bool isNull) => isEq && isNull;

    public static bool SimplifyNeqAgainstZero(bool isNeq, bool isNull) => isNeq && isNull;

    public static bool SkipZeroSimplifyWhenApprox(int op) => op == TkApproxEq;

    public static bool EmitStringCompareUsesApprox(int op) => op == TkApproxEq;

    public static int CmpCheckFlag() => 1;

    public static int CmpApproxFlag() => 2;

    public enum RegKind { Int, Float, Pointer, Other }

    public static RegKind ClassifyReg(int regTypeInt, int regTypeFloat, int regTypePointer, int actual) =>
        actual == regTypeInt ? RegKind.Int :
        actual == regTypeFloat ? RegKind.Float :
        actual == regTypePointer ? RegKind.Pointer : RegKind.Other;

    public static bool TryGetNullConstant(RegKind kind, int intVal, double floatVal, IntPtr ptr, out bool isNull)
    {
        isNull = kind switch
        {
            RegKind.Int => intVal == 0,
            RegKind.Float => floatVal == 0,
            RegKind.Pointer => ptr == IntPtr.Zero,
            _ => false,
        };
        return kind != RegKind.Other;
    }

    public static bool TryFoldConstantCompare(int op, string valueType, int intLeft, int intRight, double floatLeft, double floatRight, out bool folded)
    {
        folded = false;
        if (valueType == "string")
        {
            folded = FoldStringEqual(op == TkApproxEq, intLeft.ToString(), intRight.ToString());
            return true;
        }

        if (valueType == "float")
        {
            folded = FoldFloatEqual(op, floatLeft, floatRight);
            return true;
        }

        if (valueType == "int")
        {
            folded = FoldIntEqual(op, intLeft, intRight);
            return true;
        }

        return false;
    }

    public static bool ShouldRewriteEqToUnaryNot(int op, bool operandIsNull) =>
        op == TkEq && operandIsNull;

    public static bool ShouldRewriteNeqToBoolCast(int op, bool operandIsNull) =>
        op != TkEq && operandIsNull;

    public static bool OperandTypesNeedPromotion(bool sameType, bool leftNumeric, bool rightNumeric) =>
        !sameType && leftNumeric && rightNumeric;

    public static bool IncompatibleOperands(bool resolved) => !resolved;

    public static string ResultValueTypeBool() => "bool";

    public readonly record struct CompareEqResolveInput(
        int Op,
        bool SameType,
        bool LeftNumeric,
        bool RightNumeric,
        bool BothConstant,
        string ValueType,
        int IntLeft,
        int IntRight,
        double FloatLeft,
        double FloatRight,
        RegKind LeftReg,
        int IntVal,
        double FloatVal,
        IntPtr PtrVal,
        bool LeftStateLabel,
        bool RightNullPtr);

    public readonly record struct CompareEqResolveResult(
        bool Failed,
        bool FoldedConstant,
        bool FoldedValue,
        bool RewriteUnaryNot,
        bool RewriteBoolCast,
        bool UseTypePromotion,
        string ResultType);

    public static CompareEqResolveResult RunCompareEqResolvePipeline(CompareEqResolveInput input)
    {
        if (input.BothConstant
            && TryFoldConstantCompare(input.Op, input.ValueType, input.IntLeft, input.IntRight, input.FloatLeft, input.FloatRight, out var folded))
        {
            return new CompareEqResolveResult(false, true, folded, false, false, false, ResultValueTypeBool());
        }

        if (TryGetNullConstant(input.LeftReg, input.IntVal, input.FloatVal, input.PtrVal, out var isNull))
        {
            if (ShouldRewriteEqToUnaryNot(input.Op, isNull))
                return new CompareEqResolveResult(false, false, false, true, false, false, ResultValueTypeBool());
            if (ShouldRewriteNeqToBoolCast(input.Op, isNull))
                return new CompareEqResolveResult(false, false, false, false, true, false, ResultValueTypeBool());
        }

        if (CanCompareStateLabelToNull(input.LeftStateLabel, input.RightNullPtr))
            return new CompareEqResolveResult(false, false, true, false, false, false, ResultValueTypeBool());

        if (OperandTypesNeedPromotion(input.SameType, input.LeftNumeric, input.RightNumeric))
            return new CompareEqResolveResult(false, false, false, false, false, true, ResultValueTypeBool());

        return new CompareEqResolveResult(false, false, false, false, false, false, ResultValueTypeBool());
    }
}
