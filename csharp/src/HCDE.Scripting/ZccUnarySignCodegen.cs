namespace HCDE.Scripting;

/// <summary><c>FxPlusSign</c> / <c>FxMinusSign</c> and <c>FxTypeCast::Emit</c> from <c>codegen.cpp</c> (2025–2210).</summary>
public static class ZccUnarySignCodegen
{
    public enum MinusEmitOp
    {
        NegInt = 0,
        FlopNeg = 1,
        NegV2 = 2,
        NegV3 = 3,
        NegV4 = 4,
    }

    public enum PlusResolveOutcome
    {
        UnwrapOperand = 0,
        TypeError = 1,
    }

    public sealed record MinusResolveResult(
        bool Success,
        bool ReturnFoldedConstant,
        int? FoldedInt,
        double? FoldedFloat,
        bool NeedsBoolToIntCast);

    public sealed record MinusEmitPlan(
        bool UseKonstIntPool,
        bool EmitNegIntOpcode,
        MinusEmitOp FloatVectorOp,
        bool AllocateNewRegister,
        bool FreeSourceRegister);

    public static bool PlusSignResolveAllowed(bool numeric, bool vector, bool quaternion) =>
        numeric || vector || quaternion;

    public static PlusResolveOutcome ClassifyPlusResolve(bool numeric, bool vector, bool quaternion) =>
        PlusSignResolveAllowed(numeric, vector, quaternion)
            ? PlusResolveOutcome.UnwrapOperand
            : PlusResolveOutcome.TypeError;

    public static bool PlusSignEmitForwardsOperand() => true;

    public static bool TypeCastEmitIsUnreachable() => true;

    public static MinusResolveResult ResolveMinusSign(
        bool numeric,
        bool vector,
        bool quaternion,
        bool isConstant,
        bool isIntConstant,
        int intValue,
        double floatValue,
        bool operandIsBool)
    {
        if (!numeric && !vector && !quaternion)
            return new MinusResolveResult(false, false, null, null, false);

        if (isConstant)
        {
            if (isIntConstant)
                return new MinusResolveResult(true, true, -intValue, null, false);
            return new MinusResolveResult(true, true, null, -floatValue, false);
        }

        if (operandIsBool)
            return new MinusResolveResult(true, false, null, null, true);

        return new MinusResolveResult(true, false, null, null, false);
    }

    public static int NegateKonstIntFromPool(int regNum, Func<int, int> findConstantInt, Func<int, int> getConstantInt) =>
        -getConstantInt(findConstantInt(regNum));

    public static MinusEmitOp SelectMinusEmitOp(bool isIntReg, int regCount) =>
        isIntReg ? MinusEmitOp.NegInt : regCount switch
        {
            1 => MinusEmitOp.FlopNeg,
            2 => MinusEmitOp.NegV2,
            3 => MinusEmitOp.NegV3,
            4 => MinusEmitOp.NegV4,
            _ => MinusEmitOp.NegInt,
        };

    public static bool MinusEmitUsesInPlaceTarget(bool fromFixed) => !fromFixed;

    public static int EmitMinusFromKonstInt(int regNum, Func<int, int> findConstantInt, Func<int, int> getConstantInt, Func<int, int> allocateConstantInt) =>
        allocateConstantInt(NegateKonstIntFromPool(regNum, findConstantInt, getConstantInt));

    public static MinusEmitPlan PlanMinusEmit(bool isIntReg, int regCount, bool fromKonstInt, bool fromFixed)
    {
        if (fromKonstInt && isIntReg)
            return new MinusEmitPlan(true, false, MinusEmitOp.NegInt, false, false);

        var floatOp = SelectMinusEmitOp(isIntReg, regCount);
        if (isIntReg)
            return new MinusEmitPlan(false, true, floatOp, fromFixed, fromFixed);

        return new MinusEmitPlan(false, false, floatOp, fromFixed, fromFixed);
    }

    public static bool MinusEmitExpectsMatchingRegCount(int valueTypeRegCount, int fromRegCount) =>
        valueTypeRegCount == fromRegCount;
}
