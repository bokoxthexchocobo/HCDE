namespace HCDE.Scripting;

/// <summary><c>FxTypeCast::Emit</c>, <c>FxPlusSign</c>, <c>FxMinusSign</c> from <c>codegen.cpp</c> (2025–2210).</summary>
public static class ZccUnarySignCodegen
{
    public enum MinusEmitOpcode
    {
        NegInt = 0,
        FlopNeg = 1,
        NegV2 = 2,
        NegV3 = 3,
        NegV4 = 4,
    }

    public sealed record MinusResolveResult(
        bool Success,
        bool ReturnFoldedConstant,
        int? FoldedInt,
        double? FoldedFloat,
        bool NeedsBoolToIntCast);

    public sealed record MinusEmitResult(
        bool ReturnedKonstInt,
        int RegNum,
        bool Konst,
        MinusEmitOpcode? Opcode);

    public static bool PlusSignResolveAllowed(bool numeric, bool vector, bool quaternion) =>
        numeric || vector || quaternion;

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

    /// <summary>Mirrors <c>FxMinusSign::Emit</c> (2154–2210).</summary>
    public static MinusEmitResult EmitMinusSign(
        bool valueTypeIsInt,
        int valueTypeRegCount,
        bool fromKonst,
        bool fromRegTypeIsInt,
        int fromRegCount,
        bool fromFixed,
        int fromRegNum,
        Func<int, int> findConstantInt,
        Func<int, int> getConstantInt,
        Func<int, int> allocateConstantInt,
        Func<bool, int, int> allocateEmitRegister,
        Action<int> freeSourceRegister)
    {
        if (fromKonst && fromRegTypeIsInt)
        {
            var konstReg = allocateConstantInt(-getConstantInt(findConstantInt(fromRegNum)));
            return new MinusEmitResult(true, konstReg, true, null);
        }

        if (valueTypeRegCount != fromRegCount)
            throw new InvalidOperationException("regcount mismatch");

        int toRegNum;
        int sourceRegNum = fromRegNum;
        if (fromFixed)
        {
            toRegNum = allocateEmitRegister(fromRegTypeIsInt, fromRegCount);
            freeSourceRegister(fromRegNum);
        }
        else
        {
            toRegNum = fromRegNum;
        }

        if (valueTypeIsInt)
            return new MinusEmitResult(false, toRegNum, false, MinusEmitOpcode.NegInt);

        MinusEmitOpcode vectorOp = fromRegCount switch
        {
            1 => MinusEmitOpcode.FlopNeg,
            2 => MinusEmitOpcode.NegV2,
            3 => MinusEmitOpcode.NegV3,
            4 => MinusEmitOpcode.NegV4,
            _ => MinusEmitOpcode.NegInt,
        };

        return new MinusEmitResult(false, toRegNum, false, vectorOp);
    }
}
