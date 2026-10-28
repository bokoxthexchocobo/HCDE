namespace HCDE.Scripting;

/// <summary>Bitwise, shift, concat, and binary-logical resolve from <c>codegen.cpp</c> (4497–5082).</summary>
public static class ZccBitwiseShiftConcatLogicalResolve
{
    public const int OpAnd = '&';
    public const int OpOr = '|';
    public const int OpXor = '^';
    public const int TkLShift = 0x110;
    public const int TkRShift = 0x111;
    public const int TkUrShift = 0x112;
    public const int TkAndAnd = 0x120;
    public const int TkOrOr = 0x121;

    public const int RegtNil = 0;
    public const int RegtInt = 1;
    public const int TypeBool = 2;
    public const int TypeString = 3;
    public const int TypeName = 4;
    public const int TypeUInt32 = 5;

    public static bool BitOpBoolOperands(bool leftBool, bool rightBool) => leftBool && rightBool;

    public static bool BitOpNumericOperands(bool leftNumeric, bool rightNumeric) => leftNumeric && rightNumeric;

    public static int FoldBitOp(int op, int v1, int v2) =>
        op switch
        {
            OpAnd => v1 & v2,
            OpOr => v1 | v2,
            OpXor => v1 ^ v2,
            _ => 0,
        };

    public static int RewriteRShiftForUnsignedRight(bool leftUInt32, int version, int op, int version37) =>
        leftUInt32 && version >= version37 && op == TkRShift ? TkUrShift : op;

    public static int FoldShift(int op, int v1, int v2) =>
        op switch
        {
            _ when op == TkLShift => v1 << v2,
            _ when op == TkRShift => v1 >> v2,
            _ when op == TkUrShift => (int)((uint)v1 >> v2),
            _ => 0,
        };

    public static bool ConcatOperandValid(int regType) => regType != RegtNil;

    public static bool ConcatConstantFoldEligible(
        bool leftConst,
        bool rightConst,
        bool leftStringOrName,
        bool rightStringOrName) =>
        leftConst && rightConst && leftStringOrName && rightStringOrName;

    public static string FoldStringConcat(string left, string right) => left + right;

    public static bool NeedsBoolCast(bool isBool) => !isBool;

    public static int? LogicalAndFold(int? leftBool, int? rightBool)
    {
        if (leftBool == 0 || rightBool == 0) return 0;
        if (leftBool == 1 && rightBool == 1) return 1;
        return null;
    }

    public static LogicalPrune? LogicalAndPrune(int? leftBool, int? rightBool)
    {
        if (leftBool == 1 && rightBool is null) return LogicalPrune.TakeRight;
        if (rightBool == 1 && leftBool is null) return LogicalPrune.TakeLeft;
        return null;
    }

    public static int? LogicalOrFold(int? leftBool, int? rightBool)
    {
        if (leftBool == 1 || rightBool == 1) return 1;
        if (leftBool == 0 && rightBool == 0) return 0;
        return null;
    }

    public static LogicalPrune? LogicalOrPrune(int? leftBool, int? rightBool)
    {
        if (leftBool == 0 && rightBool is null) return LogicalPrune.TakeRight;
        if (rightBool == 0 && leftBool is null) return LogicalPrune.TakeLeft;
        return null;
    }

    public enum LogicalPrune
    {
        TakeLeft,
        TakeRight,
    }

    public static List<object?> FlattenBinaryLogical(
        int op,
        int leftExprType,
        int rightExprType,
        int binaryLogicalExprType,
        int leftOp,
        int rightOp,
        object? left,
        object? right,
        IReadOnlyList<object?>? leftList,
        IReadOnlyList<object?>? rightList)
    {
        var list = new List<object?>();
        if (leftExprType == binaryLogicalExprType && leftOp == op && leftList is not null)
            list.AddRange(leftList);
        else
            list.Add(left);

        if (rightExprType == binaryLogicalExprType && rightOp == op && rightList is not null)
            list.AddRange(rightList);
        else
            list.Add(right);
        return list;
    }
}
