namespace HCDE.Scripting;

/// <summary><c>FxPlusSign</c>/<c>FxMinusSign</c>/<c>FxUnaryNotBoolean</c> from <c>codegen.cpp</c> (2038–2385).</summary>
public static class ZccUnarySignCodegen
{
    public static bool PlusSignOperandAllowed(bool numeric, bool vector, bool quaternion) =>
        numeric || vector || quaternion;

    public static int NegateIntConstant(int value) => -value;

    public static double NegateFloatConstant(double value) => -value;

    public static bool MinusSignShouldCastBoolToInt(bool isConstant, bool isBool) =>
        !isConstant && isBool;

    public static int NegateEmitOpcode(bool isInt, int regCount) =>
        isInt ? ZccUnarySignOpcodes.OpNeg : regCount switch
        {
            1 => ZccUnarySignOpcodes.FlopNeg,
            2 => ZccUnarySignOpcodes.OpNegV2,
            3 => ZccUnarySignOpcodes.OpNegV3,
            4 => ZccUnarySignOpcodes.OpNegV4,
            _ => ZccUnarySignOpcodes.OpNeg,
        };

    public static bool UseInPlaceNegate(bool fromFixed) => !fromFixed;

    public static bool UnaryNotBooleanNeedsCast(bool operandIsBool) => !operandIsBool;

    public static bool FoldUnaryNotBoolean(bool value) => !value;

    public static int FoldUnaryNotBitwise(int value) => ~value;

    public static VmScriptFunction EmitUnaryMinusInt() =>
        new VmScriptFunction(
            [VmCode.Rrr(VmOp.Neg, 1, 0, 0), VmCode.RetInt(1)],
            [],
            [VmValueType.Integer]);

    public static VmScriptFunction EmitUnaryMinusFloat() =>
        new VmScriptFunction(
            [VmCode.Rrr(VmOp.Flop, 1, 0, 0), VmCode.RetFloat(1)],
            [],
            [VmValueType.Float]);

    public static VmScriptFunction EmitUnaryNotBoolean() =>
        new VmScriptFunction(
            [VmCode.Rrr(VmOp.Not, 1, 0, 0), VmCode.RetInt(1)],
            [],
            [VmValueType.Integer]);

    public static int NegateIntConstantFromBuilder(int regNum, Func<int, int> findConstant, Func<int, int> getConstant) =>
        -getConstant(findConstant(regNum));

    public static bool UnaryNotBooleanKonstFold(bool konst, bool isIntReg, Func<bool> readKonst, out bool folded)
    {
        folded = false;
        if (!konst || !isIntReg)
            return false;
        folded = !readKonst();
        return true;
    }

    public static bool TypeCastEmitIsUnreachable() => true;

    public static VmScriptFunction EmitUnaryMinusVector2() =>
        new VmScriptFunction(
            [VmCode.Rrr(VmOp.NegV2, 1, 0, 0), VmCode.RetFloat(1)],
            [],
            [VmValueType.Vector2]);

    public static VmScriptFunction EmitUnaryMinusVector3() =>
        new VmScriptFunction(
            [VmCode.Rrr(VmOp.NegV3, 1, 0, 0), VmCode.RetFloat(1)],
            [],
            [VmValueType.Vector3]);
}

public static class ZccUnarySignOpcodes
{
    public const int OpNeg = 1;
    public const int FlopNeg = 2;
    public const int OpNegV2 = 3;
    public const int OpNegV3 = 4;
    public const int OpNegV4 = 5;
}
